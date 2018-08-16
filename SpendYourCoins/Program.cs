// ReSharper disable All
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Protocol;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System.Collections.Generic;
using System.Linq;

namespace SpendYourCoins
{
    class Program
    {
        public static Money GetBalance(BitcoinPubKeyAddress address)
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            var balanceModel = client.GetBalance(address, true).Result;
           
            if (balanceModel == null)
                return Money.Zero;
            List<BalanceOperation> listBalance = new List<BalanceOperation>();
            foreach (var item in balanceModel.Operations)
            {
                if (item.Confirmations > 0)
                    listBalance.Add(item);
            }                   
            return new Money((decimal)listBalance.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC)), MoneyUnit.BTC); 
                       
        }

        public static List<BalanceOperation> GetPending(BitcoinPubKeyAddress address)
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            var balanceModel = client.GetBalance(address, true).Result;
           
            if (balanceModel == null)
                return null;
            List<BalanceOperation> listBalance = new List<BalanceOperation>();
            foreach (var item in balanceModel.Operations)
            {
                if (item.Confirmations == 0)
                    listBalance.Add(item);
            }
            return listBalance;

        }
        public static Dictionary<Coin, bool> GetUnspentCoins(IEnumerable<ISecret> secrets)
        {
            var unspentCoins = new Dictionary<Coin, bool>();
            foreach (var secret in secrets)
            {
                var destination = secret.PrivateKey.ScriptPubKey.GetDestinationAddress(Network.TestNet);

                var client = new QBitNinjaClient(Network.TestNet);
                var balanceModel = client.GetBalance(destination, unspentOnly: true).Result;
                foreach (var operation in balanceModel.Operations)
                {
                    foreach (var elem in operation.ReceivedCoins.Select(coin => coin as Coin))
                    {
                        unspentCoins.Add(elem, operation.Confirmations > 0);
                    }
                }
            }

            return unspentCoins;
        }

        public static OutPoint GetOutpoit(BitcoinPubKeyAddress address)
        {
            QBitNinjaClient client = new QBitNinjaClient(Network.TestNet);
            var balanceModel = client.GetBalance(address, true).Result;
            var unspentCoins = new List<OutPoint>();
            if (balanceModel.Operations.Count > 0)
            {

                foreach (var operation in balanceModel.Operations)

                    unspentCoins.AddRange(operation.ReceivedCoins.Select(coin => coin.Outpoint as OutPoint));

                //balance = unspentCoins.Sum(x => x.Amount.ToDecimal(MoneyUnit.BTC));
            }
            return unspentCoins[0];
        }
        static void Main()
        {
            

            var amount = new Money((decimal)0.0001, MoneyUnit.BTC);
            string s = SendBitcoin("cSQDiofrk7gz8w1hHArT5ZEDvRAJ5nJDFjxMi7tBuAk3tHwAyAeE", "mizpKSUZzwNG3jaFSbQf1C5k3VB7XvdZaq", amount, Network.TestNet);
           return;
          
        
            /*
           #region CiREATE NEW PRIVKEY 
           //var network = Network.TestNet;
           //Key privateKey = new Key();
           //var bitcoinPrivateKey = privateKey.GetWif(network);
           #endregion

           */

            #region IMPORT PRIVKEY
            var bitcoinPrivateKey = new BitcoinSecret("cSQDiofrk7gz8w1hHArT5ZEDvRAJ5nJDFjxMi7tBuAk3tHwAyAeE");
            var network = bitcoinPrivateKey.Network;
            #endregion

            var address = bitcoinPrivateKey.GetAddress();

            Console.WriteLine(bitcoinPrivateKey); // cSZjE4aJNPpBtU6xvJ6J4iBzDgTmzTjbq8w2kqnYvAprBCyTsG4x
            Console.WriteLine(address); // mzK6Jy5mer3ABBxfHdcxXEChsn3mkv8qJv
            Console.WriteLine();



            var client = new QBitNinjaClient(Network.TestNet);
            var transactionId = uint256.Parse("a20d27e9341f90fd0c5d6b47e1565c6eeb0b40997213f51ba7d0e747b86922aa");
            var transactionResponse = client.GetTransaction(transactionId).Result;

            Console.WriteLine(transactionResponse.TransactionId); // e44587cf08b4f03b0e8b4ae7562217796ec47b8c91666681d71329b764add2e3
            Console.WriteLine(transactionResponse.Block.Confirmations);
            Console.WriteLine();

            var receivedCoins = transactionResponse.ReceivedCoins;
            OutPoint outPointToSpend = null;
            foreach (var coin in receivedCoins)
            {
                if (coin.TxOut.ScriptPubKey == bitcoinPrivateKey.ScriptPubKey)
                {
                    outPointToSpend = coin.Outpoint;
                }
            }
            if (outPointToSpend == null)
                throw new Exception("TxOut doesn't contain our ScriptPubKey");
            Console.WriteLine("We want to spend {0}. outpoint:", outPointToSpend.N + 1);

            var transaction = new Transaction();
            transaction.Inputs.Add(new TxIn()
            {
                PrevOut = outPointToSpend
            });

            // var hallOfTheMakersAddress = new BitcoinPubKeyAddress("1KF8kUVHK42XzgcmJF4Lxz4wcL5WDL97PB");
            //var hallOfTheMakersAddress = new BitcoinPubKeyAddress("2MsNF9f3nsdkEMV6un9u3pMVKcTNJPauqaU");


            //var hallOfTheMakersAddress = new BitcoinScriptAddress("2MtBDuoyhPa8athzErp8nj4NXMKgUQ2qGq8", Network.TestNet);
            var hallOfTheMakersAddress = BitcoinAddress.Create("mizpKSUZzwNG3jaFSbQf1C5k3VB7XvdZaq");

            // How much you want to TO
            var hallOfTheMakersAmount = new Money((decimal)0.004, MoneyUnit.BTC);
            /* At the time of writing the mining fee is 0.05usd
             * Depending on the market price and
             * On the currently advised mining fee,
             * You may consider to increase or decrease it
            */
            var minerFee = new Money((decimal)0.0001, MoneyUnit.BTC);
            // How much you want to spend FROM
            var txInAmount = (Money)receivedCoins[(int)outPointToSpend.N].Amount;
            Money changeBackAmount = txInAmount - hallOfTheMakersAmount - minerFee;

            Money balace = GetBalance(bitcoinPrivateKey.GetAddress());
            List < BalanceOperation > list = GetPending(bitcoinPrivateKey.GetAddress());
            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = hallOfTheMakersAmount,
                ScriptPubKey = hallOfTheMakersAddress.ScriptPubKey
            };

            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = bitcoinPrivateKey.ScriptPubKey
            };

            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);
            

            var message = "nopara73 loves NBitcoin!";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });

            //Console.WriteLine(transaction);

            //var address = new BitcoinPubKeyAddress("mzK6Jy5mer3ABBxfHdcxXEChsn3mkv8qJv");
            //transaction.Inputs[0].ScriptSig = address.ScriptPubKey;

            // It is also OK:
            transaction.Inputs[0].ScriptSig = bitcoinPrivateKey.ScriptPubKey;
            transaction.Sign(bitcoinPrivateKey, false);

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                Console.WriteLine(string.Format("ErrorCode: {0}", broadcastResponse.Error.ErrorCode));
                Console.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                Console.WriteLine(transaction.GetHash());
            }

            //using (var node = Node.ConnectToLocal(network)) //Connect to the node
            //{
            //    node.VersionHandshake(); //Say hello
            //                             //Advertize your transaction (send just the hash)
            //    node.SendMessage(new InvPayload(InventoryType.MSG_TX, transaction.GetHash()));
            //    //Send it
            //    node.SendMessage(new TxPayload(transaction));
            //    Thread.Sleep(500); //Wait a bit
            //}


            Console.ReadLine();

            Console.ReadLine();
        }
        /// <summary>
        /// Truong Tran Van 
        /// Send func send bitcoin 
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <param name="network"></param>
        /// <returns></returns>
        public static string SendBitcoin(string privateKey, string address, Money amount , Network network)
        {        

            //Get SecretPrivate 
            BitcoinSecret oBitcoinSecret = new BitcoinSecret(privateKey, network);     
            List<BalanceOperation> list = GetPending(oBitcoinSecret.GetAddress());
            //2. Check balace comparse amount 
            Money objBalace = GetBalance(oBitcoinSecret.GetAddress());
            if (amount > objBalace)
                return "Not Enought bitcoin";
            //3. Get last transection 
            QBitNinjaClient client = new QBitNinjaClient(network);           
           var balanceModel = client.GetBalance(oBitcoinSecret.GetAddress(), true).Result;       
            
            var transaction = new Transaction();
            Money oMoneySpend = 0;
            foreach (var operation  in balanceModel.Operations)
            {
                TxIn txInput = new TxIn();
                txInput.ScriptSig = oBitcoinSecret.ScriptPubKey;
                txInput.PrevOut = operation.ReceivedCoins[0].Outpoint;
                transaction.AddInput(txInput);
                oMoneySpend += operation.Amount;
                if (oMoneySpend>=amount)
                {
                    break;
                }
            }
            //TxOut txOut = new TxOut();
            //Money fee = Money.Satoshis(40000);
            //txOut.Value = amount - fee;
            //var BitCoinAddress = BitcoinAddress.Create(address);
            //txOut.ScriptPubKey = BitCoinAddress.ScriptPubKey;
            //transaction.AddOutput(txOut);
            //transaction.Inputs[].ScriptSig
            //transaction.Sign(oBitcoinSecret, false);
            var hallOfTheMakersAddress = BitcoinAddress.Create(address);
            // How much you want to TO
            var hallOfTheMakersAmount = amount; new Money((decimal)0.1643, MoneyUnit.BTC);
            /* At the time of writing the mining fee is 0.05usd
             * Depending on the market price and
             * On the currently advised mining fee,
             * You may consider to increase or decrease it
            */
            var minerFee = new Money((decimal)0.000005, MoneyUnit.BTC);
            hallOfTheMakersAmount -= minerFee;
            // How much you want to spend FROM
            var txInAmount = oMoneySpend;//(Money)receivedCoins[(int)outPointToSpend.N].Amount;
            Money changeBackAmount = txInAmount - hallOfTheMakersAmount - minerFee;

            //Money balace = CheckBalance(bitcoinPrivateKey.GetAddress());

            TxOut hallOfTheMakersTxOut = new TxOut()
            {
                Value = hallOfTheMakersAmount,
                ScriptPubKey = hallOfTheMakersAddress.ScriptPubKey
            };

            TxOut changeBackTxOut = new TxOut()
            {
                Value = changeBackAmount,
                ScriptPubKey = oBitcoinSecret.ScriptPubKey
            };

            transaction.Outputs.Add(hallOfTheMakersTxOut);
            transaction.Outputs.Add(changeBackTxOut);

            var message = "nopara73 loves NBitcoin!";
            var bytes = Encoding.UTF8.GetBytes(message);
            transaction.Outputs.Add(new TxOut()
            {
                Value = Money.Zero,
                ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
            });
            
            transaction.Inputs[0].ScriptSig = oBitcoinSecret.ScriptPubKey;
            transaction.Sign(oBitcoinSecret, false);                    

            BroadcastResponse broadcastResponse = client.Broadcast(transaction).Result;

            if (!broadcastResponse.Success)
            {
                Console.WriteLine(string.Format("ErrorCode: {0}", broadcastResponse.Error.ErrorCode));
                Console.WriteLine("Error message: " + broadcastResponse.Error.Reason);
            }
            else
            {
                Console.WriteLine("Success! You can check out the hash of the transaciton in any block explorer:");
                Console.WriteLine(transaction.GetHash());
            }            

            return "true";
        }

      
    }
}
