
using System;

namespace socket4net
{
    public class OrderAttribute : Attribute
    {
        public int Order { get; private set; }
        public OrderAttribute(int order)
        {
            Order = order;
        }
    }
}
