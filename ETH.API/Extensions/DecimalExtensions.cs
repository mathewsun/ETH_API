namespace ETH.API.Extensions
{
    public static class DecimalExtensions
    {
        //https://www.eth-to-wei.com/?gclid=CjwKCAjwv4SaBhBPEiwA9YzZvCYRctTFKZi4Dty5_KeL9XncDNdwGeRRR7WfwOqsaT4un1ii7cvkuRoCx6kQAvD_BwE
        public static decimal WeiToEth(this decimal value)
        {
            return value / 1000000000000000000;
        }

        public static decimal EthToWei(this decimal value)
        {
            return value * 1000000000000000000;
        }
    }
}
