using System;
using NBitcoin;
// ReSharper disable All

namespace PrivateKey
{
    class Program
    {
        static void Main()
        {
           
            Key privateKey = new Key(); // generate a random private key
            BitcoinSecret mainNetPrivateKey = privateKey.GetBitcoinSecret(Network.Main);  // get our private key for the mainnet
            BitcoinSecret testNetPrivateKey = privateKey.GetBitcoinSecret(Network.TestNet);  // get our private key for the testnet
            Console.WriteLine(mainNetPrivateKey); // L5B67zvrndS5c71EjkrTJZ99UaoVbMUAK58GKdQUfYCpAa6jypvn
            Console.WriteLine(mainNetPrivateKey.GetAddress());

            Console.WriteLine(testNetPrivateKey); // cVY5auviDh8LmYUW8conAfafseD6p6uFoZrP7GjS3rzAerpRKE9Wmuz
            Console.WriteLine(testNetPrivateKey.GetAddress());
            bool WifIsBitcoinSecret = mainNetPrivateKey == privateKey.GetWif(Network.Main);
            Console.WriteLine(WifIsBitcoinSecret); // True

            
            BitcoinSecret bitcoinPrivateKey = privateKey.GetWif(Network.Main); // L5B67zvrndS5c71EjkrTJZ99UaoVbMUAK58GKdQUfYCpAa6jypvn
            Key samePrivateKey = bitcoinPrivateKey.PrivateKey;

            PubKey publicKey = privateKey.PubKey;
            BitcoinPubKeyAddress bitcoinPubicKey = publicKey.GetAddress(Network.Main); // 1PUYsjwfNmX64wS368ZR5FMouTtUmvtmTY
            //PubKey samePublicKey = bitcoinPubicKey.ItIsNotPossible;

            Console.ReadLine();
        }
    }
}
