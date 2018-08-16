using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var secret=   new BitcoinSecret("L4jwDMee1kkysYp2D2yuaW3HLhtF6fFHssbZAANPR1wEMKmDVG2P1111");

            var key = secret.PrivateKey;
            Console.WriteLine(key.PubKey.GetAddress(Network.Main));
            Transaction tx = new Transaction();
            var input = new TxIn();
            input.ScriptSig = secret.GetAddress().ScriptPubKey;
            tx.AddInput(input);

            TxOut output = new TxOut();
            var description = BitcoinAddress.Create("1BqaNepUZhK5nTJnDiirgTdCDg2LWNb5rT");
            Money fee = Money.Satoshis(40000);
            output.Value = Money.Coins(0.1m)-fee;
            output.ScriptPubKey = description.ScriptPubKey;
            tx.AddOutput(output);

             tx.Sign(secret);

        }
    }
}
