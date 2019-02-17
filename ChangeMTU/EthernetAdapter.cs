namespace ChangeMTU
{
    /** 
     * Value object for ethernet adapter info
     * */
    class EthernetAdapter
    {
        public int Index { get; }
        public long MTU { get; set; }
        public string Status { get; set; }
        public string Name { get; }
        public string DisplayName { get{ return string.Format(@"{0} ({1})", Name, Status); } }

        public EthernetAdapter(int index, long mtu, string status, string name) {
            Index = index;
            MTU = mtu;
            Status = status;
            Name = name;
        }

    }
}
