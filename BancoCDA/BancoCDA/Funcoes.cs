using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using NativeUI;
using BancoCDA.Classes.Mod;
using BancoCDA.Classes.Banking;
using Newtonsoft.Json;
using System.IO;
using GTA.Math;
using System.Drawing;

namespace BancoCDA
{
    public class SBS : Script
    {
        
        private static List<Blip> blips = new List<Blip>(); 
        private static List<object> bankTypes = new List<object>(); 
        private static List<object> bankAccounts = new List<object>(); 

        
        private static MenuPool pool = new MenuPool(); 

        
        private static UIMenu bankCreationMenu = new UIMenu("Criar um banco", "SELECIONE UMA OPÇÃO"); 
        private static UIMenuListItem bankType; 
        private static UIMenuItem createBank = new UIMenuItem("Criar banco na posição atual"); 

        
        private static UIMenu bankMenu = new UIMenu("Banco", "SELECIONE UMA OPÇÃO"); 
        private static UIMenu manageAccount = new UIMenu("Gerenciar conta", "SELECIONE UMA OPÇÃO");
        private static UIMenu transferMenu = new UIMenu("Transferir fundos", "SELECIONE UMA OPÇÃO");
        private static UIMenuListItem bankAccountList; 
        private static UIMenuItem deposit = new UIMenuItem("Deposito"); 
        private static UIMenuItem withdraw = new UIMenuItem("Retirar"); 
        private static UIMenuItem transfer = new UIMenuItem("Trnsferir"); 
        private static UIMenuItem createAccount = new UIMenuItem("Abrir Conta"); 
        private static UIMenuItem closeAccount = new UIMenuItem("Fechar Conta"); 
        private static UIMenuItem balance = new UIMenuItem("Balance: $0"); 

        private static readonly string modDirectory = "scripts\\BancoCDA";
        private static readonly string bankDirectory = "scripts\\BancoCDA\\Accounts"; 
        private static readonly string modConfig = "scripts\\BancoCDA\\configuration.json"; 
        private static readonly string franklinsBank = "scripts\\BancoCDA\\Accounts\\franklin.json"; 
        private static readonly string michaelsBank = "scripts\\BancoCDA\\Accounts\\michael.json"; 
        private static readonly string trevorsBank = "scripts\\BancoCDA\\Accounts\\trevor.json"; 
        private static readonly string globalBank = "scripts\\BancoCDA\\Accounts\\global.json"; 

        
        private static BankAccount franklinsAccount; 
        private static BankAccount michaelsAccount; 
        private static BankAccount trevorsAccount; 
        private static BankAccount globalAccount; 
        private static Configuration configuration; 
        private static Bank currentBank; 

        public SBS()
        {
            try
            {
                
                Tick += OnTick; 
                KeyDown += OnKeyDown; 
                Aborted += OnAbort; 

                
                VerifyDirectories(); 
                Load(); 
                SetupMenus(); 

                UI.Notify($@"~y~BancoCDA 0.1 ~w~carregado com sucesso!"); 
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void VerifyDirectories()
        {
            try
            {
                if (!Directory.Exists(modDirectory)) Directory.CreateDirectory(modDirectory); 
                if (!Directory.Exists(bankDirectory)) Directory.CreateDirectory(bankDirectory); 

                if (!File.Exists(modConfig)) 
                {
                    configuration = new Configuration() 
                    {
                        banks = new List<Bank>(), 
                        settings = new Settings() 
                        {
                            addBankKey = Keys.N, 
                            addBankRequiresShift = true 
                        }
                    };

                    File.WriteAllText(modConfig, JsonConvert.SerializeObject(configuration, Formatting.Indented)); 
                }

                if (!File.Exists(franklinsBank)) 
                {
                    BankAccount account = new BankAccount() 
                    {
                        balance = 0, 
                        owner = AccountOwner.franklin, 
                        transactions = new List<Transaction>(), 
                        isOpened = false, 
                        bank = null 
                    };
                    File.WriteAllText(franklinsBank, JsonConvert.SerializeObject(account, Formatting.Indented)); 
                }
                else franklinsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(franklinsBank)); 
                if (!File.Exists(michaelsBank)) 
                {
                    BankAccount account = new BankAccount() 
                    {
                        balance = 0, 
                        owner = AccountOwner.michael, 
                        transactions = new List<Transaction>(), 
                        isOpened = false, 
                        bank = null 
                    };
                    File.WriteAllText(michaelsBank, JsonConvert.SerializeObject(account, Formatting.Indented));
                }
                else michaelsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(michaelsBank)); 

                if (!File.Exists(trevorsBank)) 
                {
                    BankAccount account = new BankAccount() 
                    {
                        balance = 0, 
                        owner = AccountOwner.trevor, 
                        transactions = new List<Transaction>(), 
                        isOpened = false, 
                        bank = null 
                    };
                    File.WriteAllText(trevorsBank, JsonConvert.SerializeObject(account, Formatting.Indented)); 
                }
                else trevorsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(trevorsBank)); 

                if (!File.Exists(globalBank)) 
                {
                    BankAccount account = new BankAccount() 
                    {
                        balance = 0, 
                        owner = AccountOwner.global, 
                        transactions = new List<Transaction>(), 
                        isOpened = false, 
                        bank = null 
                    };
                    File.WriteAllText(globalBank, JsonConvert.SerializeObject(account, Formatting.Indented)); 
                }
                else globalAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(globalBank)); 
            }
            catch (Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void SetupMenus()
        {
            try
            {
                
                bankTypes.Add("Fleeca"); 
                bankTypes.Add("Pacific Standard"); 
                bankTypes.Add("Blaine County"); 
                bankTypes.Add("Generic");
                bankType = new UIMenuListItem("Type:", bankTypes, 1); 
                createBank.Activated += MenuItemPressed;
                bankCreationMenu.AddItem(bankType);
                bankCreationMenu.AddItem(createBank); 
                pool.Add(bankCreationMenu); 

                
                bankAccounts.Add("Franklin"); 
                bankAccounts.Add("Michael"); 
                bankAccounts.Add("Trevor"); 
                bankAccounts.Add("Global"); 
                manageAccount = pool.AddSubMenu(bankMenu, "Gerenciar conta"); 
                manageAccount.AddItem(createAccount); 
                manageAccount.AddItem(closeAccount); 
                bankAccountList = new UIMenuListItem("Conta de destino:", bankAccounts, 1);
                createAccount.Activated += MenuItemPressed; 
                closeAccount.Activated += MenuItemPressed; 
                withdraw.Activated += MenuItemPressed; 
                deposit.Activated += MenuItemPressed; 
                transfer.Activated += MenuItemPressed; 
                bankMenu.AddItem(deposit); 
                bankMenu.AddItem(withdraw); 
                transferMenu = pool.AddSubMenu(bankMenu, "Transferir"); 
                transferMenu.AddItem(bankAccountList); 
                transferMenu.AddItem(transfer); 
                bankMenu.AddItem(balance); 
                pool.Add(bankMenu); 
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void MenuItemPressed(UIMenu sender, UIMenuItem selectedItem)
        {
            try
            {
                BankAccount currAccount; 
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; 
                else currAccount = globalAccount; 

                int currentAmount = 0;

                if (selectedItem.Text == createBank.Text) 
                {
                    if (bankType.Items[bankType.Index].ToString() == "Fleeca") CreateBankAtPlayerPosition(BankType.fleeca); 
                    else if (bankType.Items[bankType.Index].ToString() == "Pacific Standard") CreateBankAtPlayerPosition(BankType.pacificStandard); 
                    else if (bankType.Items[bankType.Index].ToString() == "Blaine County") CreateBankAtPlayerPosition(BankType.blaineCounty);
                }
                else if (selectedItem.Text == deposit.Text)
                {
                    currentAmount = GetNumberInput(); 

                    if (Game.Player.Money >= currentAmount)
                    {
                        if (currentAmount >= 1)
                        {
                            Transaction transaction = new Transaction() 
                            {
                                transactionAmount = currentAmount,
                                transactionDate = World.CurrentDate,
                                transactionText = "DEPOSITO FEITO COM SUCESSO.",
                                type = TransactionType.deposit
                            };
                            Game.Player.Money = Game.Player.Money - currentAmount; 
                            currAccount.balance = currAccount.balance + currentAmount; 
                            currAccount.transactions.Add(transaction); 
                            UI.Notify($"Depositado com sucesso ~y~${currentAmount}~w~ para sua conta!"); 
                        }
                    }
                    else
                    {
                        UI.Notify("~r~Error: Você não possui os fundos necessários para fazer esta transação"); 
                    }
                }
                else if (selectedItem.Text == withdraw.Text)
                {
                    currentAmount = GetNumberInput(); 

                    if (currAccount.balance >= currentAmount)
                    {
                        if (currentAmount >= 1)
                        {
                            Transaction transaction = new Transaction() 
                            {
                                transactionAmount = currentAmount,
                                transactionDate = World.CurrentDate,
                                transactionText = "RETIRADA EMITIDA PELO PROPRIETÁRIO DA CONTA",
                                type = TransactionType.withdrawal
                            };
                            Game.Player.Money = Game.Player.Money + currentAmount; 
                            currAccount.balance = currAccount.balance - currentAmount; 
                            currAccount.transactions.Add(transaction); 
                            UI.Notify($"Retirado com sucesso ~y~${currentAmount}~w~ da sua conta!"); 
                        }
                    }
                    else
                    {
                        UI.Notify("~r~Error: Você não possui os fundos necessários para fazer esta transação"); 
                    }
                }
                else if (selectedItem.Text == closeAccount.Text)
                {
                    if (currAccount.isOpened && currentBank.typeOfBank == currAccount.bank.typeOfBank)
                    {
                        Game.Player.Money = Game.Player.Money + currAccount.balance - currAccount.bank.startingMoney / 2; 
                        currAccount.balance = 0; 
                        currAccount.isOpened = false; 
                        currAccount.bank = null; 
                        UI.Notify($"Você encerrou sua conta com sucesso!"); 
                    }
                    else
                    {
                        string bankname = null; 
                        if (currAccount.bank.typeOfBank == BankType.fleeca) bankname = "Fleeca"; 
                        else if (currAccount.bank.typeOfBank == BankType.blaineCounty) bankname = "Blaine County Savings"; 
                        else if (currAccount.bank.typeOfBank == BankType.pacificStandard) bankname = "Pacific Standard"; 
                        UI.Notify($"~r~Você não pode fechar sua conta aqui! vá para (a) {bankname} banco para fechar sua conta"); 
                    }
                }
                else if (selectedItem.Text == createAccount.Text)
                {
                    if (!currAccount.isOpened)
                    {
                        currAccount.bank = currentBank; 
                        currAccount.balance = currentBank.startingMoney; 
                        currAccount.isOpened = true; 
                        Game.Player.Money = Game.Player.Money - currentBank.startingMoney / 2; 
                        string bankname = null; 
                        if (currentBank.typeOfBank == BankType.fleeca) bankname = "Fleeca"; 
                        else if (currentBank.typeOfBank == BankType.blaineCounty) bankname = "Blaine County Savings"; 
                        else if (currentBank.typeOfBank == BankType.pacificStandard) bankname = "Pacific Standard"; 
                        UI.Notify($"Você abriu uma conta com sucesso ~y~{bankname}~w~!");
                    }
                }
                else if (selectedItem.Text == transfer.Text)
                {
                    int amountToTransfer = GetNumberInput(); 

                    if (currAccount.balance >= amountToTransfer)
                    {
                        if (bankAccountList.Items[bankAccountList.Index].ToString() == "Franklin" && currAccount != franklinsAccount && franklinsAccount.isOpened) franklinsAccount.balance = franklinsAccount.balance + amountToTransfer; 
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Michael" && currAccount != michaelsAccount && michaelsAccount.isOpened) michaelsAccount.balance = michaelsAccount.balance + amountToTransfer; 
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Trevor" && currAccount != trevorsAccount && trevorsAccount.isOpened) trevorsAccount.balance = trevorsAccount.balance + amountToTransfer; 
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Global" && currAccount != globalAccount && globalAccount.isOpened) globalAccount.balance = globalAccount.balance + amountToTransfer; 
                        else
                        {
                            UI.Notify("A conta selecionada não está aberta!"); 
                            return; 
                        }

                        currAccount.balance = currAccount.balance - amountToTransfer; 

                        UI.Notify($"Transferido com sucesso ~y~${amountToTransfer}~w~ to ~y~{bankAccountList.Items[bankAccountList.Index].ToString()}~w~!"); 
                    }
                    else
                    {
                        UI.Notify("~r~Error: Você não possui os fundos necessários para fazer esta transação"); 
                    }
                }

                if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Salvando alterações");
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; 
                else currAccount = globalAccount; 
                Save(); 
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void CreateBankAtPlayerPosition(BankType typeOfBank)
        {
            try
            {
                Vector3 currentPos = Game.Player.Character.Position;
                Bank newBank = new Bank() 
                {
                    XPos = currentPos.X, 
                    YPos = currentPos.Y, 
                    ZPos = currentPos.Z, 
                    typeOfBank = typeOfBank 
                };
                if (newBank.typeOfBank == BankType.fleeca) newBank.startingMoney = 250; 
                else if (newBank.typeOfBank == BankType.blaineCounty) newBank.startingMoney = 50; 
                else if (newBank.typeOfBank == BankType.pacificStandard) newBank.startingMoney = 500; 
                configuration.banks.Add(newBank); 
                Save(); 
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um Erro: {ex.Message}");
            }
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(michaelsBank, JsonConvert.SerializeObject(michaelsAccount, Formatting.Indented)); 
                File.WriteAllText(franklinsBank, JsonConvert.SerializeObject(franklinsAccount, Formatting.Indented)); 
                File.WriteAllText(trevorsBank, JsonConvert.SerializeObject(trevorsAccount, Formatting.Indented)); 
                File.WriteAllText(globalBank, JsonConvert.SerializeObject(globalAccount, Formatting.Indented)); 
                File.WriteAllText(modConfig, JsonConvert.SerializeObject(configuration, Formatting.Indented)); 
                if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Salva todas as contas e a configuração");
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                michaelsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(michaelsBank)); 
                franklinsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(franklinsBank)); 
                trevorsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(trevorsBank)); 
                globalAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(globalBank)); 
                configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(modConfig)); 

                foreach (Bank bank in configuration.banks) 
                {
                    Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos); 
                    Blip blp = World.CreateBlip(bankPos);
                    blp.IsShortRange = true; 
                    blp.Sprite = BlipSprite.Store; 
                    blp.Color = BlipColor.Green; 
                    if (bank.typeOfBank == BankType.fleeca) blp.Name = "Fleeca Bank"; 
                    else if (bank.typeOfBank == BankType.blaineCounty) blp.Name = "Blaine County Bank"; 
                    else if (bank.typeOfBank == BankType.pacificStandard) blp.Name = "Pacific Standard Bank"; 
                    blips.Add(blp); 
                    if (configuration.settings.showDebugMessages) UI.Notify($"DEBUG: Banco de carregamento: {bank.typeOfBank.ToString()} at {bankPos.ToString()}");
                }
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                BankAccount currAccount; 
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; 
                else currAccount = globalAccount; 

                if (currAccount.isOpened)
                {
                    if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Ativar itens de menu de depósito, retirada, saldo e abertura de conta");
                    
                    deposit.Enabled = true;
                    withdraw.Enabled = true;
                    transfer.Enabled = true;
                    balance.Enabled = true;
                    createAccount.Enabled = false; 
                    closeAccount.Enabled = true; 
                }
                else
                {
                    if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Desativando itens de menu de depósito, retirada, saldo e fechamento de conta");
                   
                    deposit.Enabled = false;
                    withdraw.Enabled = false;
                    transfer.Enabled = false;
                    balance.Enabled = false;
                    createAccount.Enabled = true;
                    closeAccount.Enabled = false; 
                }



                balance.Text = $"Balance: ${currAccount.balance}"; 

                if (pool != null) pool.ProcessMenus(); 
                foreach (Bank bank in configuration.banks)
                {
                    Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos - 1); 
                    Vector3 bankPosNM = new Vector3(bank.XPos, bank.YPos, bank.ZPos); 
                    if (World.GetDistance(Game.Player.Character.Position, bankPosNM) <= .75f && !bankMenu.Visible && !manageAccount.Visible && !transferMenu.Visible) 
                    {
                        UI.ShowSubtitle("Pressione ~y~E~w~ para usar o banco", 1); 
                    }
                    if (bankMenu.Visible || manageAccount.Visible) Game.Player.Character.FreezePosition = true; 
                    else if (!bankMenu.Visible && !manageAccount.Visible) Game.Player.Character.FreezePosition = false; 
                    World.DrawMarker(MarkerType.VerticalCylinder, bankPos, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Color.Yellow); 
                }
            }
            catch (Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private void OnAbort(object sender, EventArgs e)
        {
            foreach(Blip blip in blips) 
            {
                blip.Remove(); 
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Shift) 
                {
                    if (e.KeyCode == Keys.N && Game.Player.Character.CurrentVehicle == null) 
                    {
                        bankCreationMenu.Visible = !bankCreationMenu.Visible;
                    }
                }
                if (e.KeyCode == Keys.E) 
                {
                    foreach (Bank bank in configuration.banks)
                    {
                        Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos); 
                        if (World.GetDistance(Game.Player.Character.Position, bankPos) <= .75f) 
                        {
                            currentBank = bank; 
                            bankMenu.Visible = !bankMenu.Visible; 
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        private int GetNumberInput()
        {
            try
            {
                int number = 0;
            returnpoint:
                try
                {
                    Int32.TryParse(Game.GetUserInput("", 7), out number); 
                }
                catch 
                {
                    UI.Notify("Entrada inválida. Por favor, tente novamente"); 
                    Wait(500); 
                    goto returnpoint; 
                }
                return number;
            }
            catch (Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
                return 0;
            }
        }

        public static void ChargePlayerAccount(int amount, string bankLogText = "PAGAMENTO EMITIDO POR SCRIPT EXTERNO")
        {
            try
            {
                BankAccount currAccount; 
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; 
                else currAccount = globalAccount; 

                if (currAccount.balance >= amount)
                {
                    Transaction transaction = new Transaction() 
                    {
                        transactionAmount = amount,
                        transactionDate = World.CurrentDate,
                        transactionText = bankLogText,
                        type = TransactionType.withdrawal
                    };

                    currAccount.balance = currAccount.balance - amount;

                    currAccount.transactions.Add(transaction); 
                }

                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount;
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount;
                else currAccount = globalAccount;
                Save();
            }
            catch (Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }

        public static void AddToPlayerAccount(int amountToAdd, string bankLogText = "DEPÓSITO EMITIDO POR SCRIPT EXTERNO")
        {
            try
            {
                BankAccount currAccount; 
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; 
                else currAccount = globalAccount; 

                Transaction transaction = new Transaction() 
                {
                    transactionAmount = amountToAdd,
                    transactionDate = World.CurrentDate,
                    transactionText = bankLogText,
                    type = TransactionType.deposit
                };

                currAccount.balance = currAccount.balance + amountToAdd; 

                currAccount.transactions.Add(transaction); 

                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; 
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; 
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount;
                else currAccount = globalAccount;
                Save(); 
            }
            catch (Exception ex)
            {
                UI.Notify($"Ocorreu um erro: {ex.Message}");
            }
        }
    }
}
