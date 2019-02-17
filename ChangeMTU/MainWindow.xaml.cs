using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ChangeMTU
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public const Int32 MTU_DEFAULT = 1500;
        public const Int32 MTU_CUTDOWN = 400;

        // List of Ethernet adapters
        private static List<EthernetAdapter> EthernetAdapters = null;

        /** 
         * UI Main 
         * */
        public MainWindow()
        {
            InitializeComponent();

            // initilise ethernet adapter combobox
            list_eth.DisplayMemberPath = "DisplayName";
            list_eth.SelectedValuePath = "Index";

            // refresh list
            refreshEthernetAdapterList();
        }
        
        /**
         * Refresh button event listsner
         * */
        private void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            refreshEthernetAdapterList();
        }

        /**
         * Ethernet adapter selected
         */
        private void List_eth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setMtuInfo(list_eth.SelectedItem);
        }

        /**
         * Refresh ethernet adapter list
         * */
        private void refreshEthernetAdapterList() {
            var prevSelect = list_eth.SelectedValue;
            NetUtil.RetrieveEthernetAdapterList((List<EthernetAdapter> ethList) => {
                if (!this.Dispatcher.CheckAccess())
                {
                    // refresh ethernet adapter list
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        updateEthernetAdapterList(ethList, prevSelect);
                        setMtuInfo(list_eth.SelectedItem, true);
                    }));
                }
                else
                {
                    updateEthernetAdapterList(ethList, prevSelect);
                    setMtuInfo(list_eth.SelectedItem, true);
                }
            });
        }

        /**
         * update ui - MTU panel
         * */
        private void updateEthernetAdapterList(List<EthernetAdapter> ethList, object prevSelect) {
            // disable onchanged listener
            list_eth.SelectionChanged -= List_eth_SelectionChanged;

            // Set new list
            EthernetAdapters = ethList;
            list_eth.ItemsSource = EthernetAdapters;

            // set selected default or previous value
            if (prevSelect != null)
            {
                list_eth.SelectedValue = prevSelect;
            }
            else if (EthernetAdapters?.Count > 0)
            {
                list_eth.SelectedValue = EthernetAdapters[0].Index;
            }

            // enable onchanged listener
            list_eth.SelectionChanged += List_eth_SelectionChanged;
        }

        /**
         * Show mtu info
         * */
        private static Int32 currentAdapterIndex = -1;
        private void setMtuInfo(object item, Boolean forceRefresh = false) {
            EthernetAdapter eth = item as EthernetAdapter;
            if (eth == null)
            {
                currentAdapterIndex = -1;
                border_mtu_status.Visibility = Visibility.Hidden;
            } 
            else if(forceRefresh || currentAdapterIndex != eth.Index)
            {
                currentAdapterIndex = eth.Index;
                border_mtu_status.Visibility = Visibility.Visible;
                lbl_mtu_value.Content = String.Format(@"Current MTU : {0}", eth.MTU);
                if (eth.MTU == 1500)
                {
                    border_mtu_status.BorderBrush = new SolidColorBrush(Colors.Gray);
                    lbl_mtu_value.Foreground = Brushes.Gray;
                    lbl_mtu_status.Foreground = Brushes.Gray;
                    lbl_mtu_status.Content = "Default";
                }
                else
                {
                    border_mtu_status.BorderBrush = new SolidColorBrush(Colors.Red);
                    lbl_mtu_value.Foreground = Brushes.Red;
                    lbl_mtu_status.Foreground = Brushes.Red;
                    lbl_mtu_status.Content = "Adapted";
                }
            }
        }

        /**
         * Toggle MTU click
         * */
        private void mtu_change_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Debug.Print("index : {0}",(list_eth.SelectedItem as EthernetAdapter)?.Index);

            EthernetAdapter eth = list_eth.SelectedItem as EthernetAdapter;
            if (eth != null) {
                if(eth.MTU == 1500)
                {
                    NetUtil.ChangeMTU(eth.Index, MTU_CUTDOWN, null);
                }
                else
                {
                    NetUtil.ChangeMTU(eth.Index, MTU_DEFAULT, null);
                }
                refreshEthernetAdapterList();
            }
        }
    }
}
