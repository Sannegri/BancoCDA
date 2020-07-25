using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using NativeUI;
using Starmans_Banking_System.Classes.Mod;
using Starmans_Banking_System.Classes.Banking;
using Newtonsoft.Json;
using System.IO;
using GTA.Math;
using System.Drawing;

namespace Starmans_Banking_System
{
    public class SBS : Script
    {
        // Define all Lists
        private static List<Blip> blips = new List<Blip>(); // Create a list of blips to be used to remove all blips created by this mod when the mod is closed or reloaded
        private static List<object> bankTypes = new List<object>(); // Create a list of bank types to be used in the bank creation menu
        private static List<object> bankAccounts = new List<object>(); // Create a list of bank accounts to be used in the transfer funds menu

        // Define all UI Menu variables
        private static MenuPool pool = new MenuPool(); // Create a menupool for all UI menus

        // Bank Creation Variables
        private static UIMenu bankCreationMenu = new UIMenu("Create A Bank", "SELECT AN OPTION"); // Create the UI menu that will be shown when the player is creating a new bank
        private static UIMenuListItem bankType; // Create but don't define the UIMenuListItem for the bank type to create
        private static UIMenuItem createBank = new UIMenuItem("Create Bank At Current Position"); // Create the UI Menu Item to be displayed in the create a bank menu to create a bank

        // Bank Menu Variables
        private static UIMenu bankMenu = new UIMenu("Bank", "SELECT AN OPTION"); // Create a UI Menu for banks
        private static UIMenu manageAccount = new UIMenu("Manage Account", "SELECT AN OPTION");
        private static UIMenu transferMenu = new UIMenu("Transfer Funds", "SELECT AN OPTION");
        private static UIMenuListItem bankAccountList; // Create a list item to be used in the transfer menu
        private static UIMenuItem deposit = new UIMenuItem("Deposit"); // Create a deposit button
        private static UIMenuItem withdraw = new UIMenuItem("Withdraw"); // Create a withdraw button
        private static UIMenuItem transfer = new UIMenuItem("Transfer"); // Create a transfer button
        private static UIMenuItem createAccount = new UIMenuItem("Open Account"); // Create an Open Account button
        private static UIMenuItem closeAccount = new UIMenuItem("Close Account"); // Create a Close Account button
        private static UIMenuItem balance = new UIMenuItem("Balance: $0"); // Create a balance button (this has no actions, this is simply a display)

        // Define all filesystem variables
        private static readonly string modDirectory = "scripts\\StarmansBankSystem"; // Create a string containing the path to the directory where all (non-library) files used by this mod will be
        private static readonly string bankDirectory = "scripts\\StarmansBankSystem\\Accounts"; // Create a string containing the path to the directory where bank accounts are stored
        private static readonly string modConfig = "scripts\\StarmansBankSystem\\configuration.json"; // Create a string containing the path to the mod config
        private static readonly string franklinsBank = "scripts\\StarmansBankSystem\\Accounts\\franklin.json"; // Create a string containing the path to Franklins bank info
        private static readonly string michaelsBank = "scripts\\StarmansBankSystem\\Accounts\\michael.json"; // Create a string containing the path to Michaels bank info
        private static readonly string trevorsBank = "scripts\\StarmansBankSystem\\Accounts\\trevor.json"; // Create a string containing the path to Trevors bank info
        private static readonly string globalBank = "scripts\\StarmansBankSystem\\Accounts\\global.json"; // Create a string containing the path to the global bank info (bank info for all non story characters)

        // Define all misc variables
        private static BankAccount franklinsAccount; // Create a bank account variable for Franklins information
        private static BankAccount michaelsAccount; // Create a bank account variable for Michaels information
        private static BankAccount trevorsAccount; // Create a bank account variable for Trevors information
        private static BankAccount globalAccount; // Create a bank account variable for the global account
        private static Configuration configuration; // Create a variable for the mod configuration
        private static Bank currentBank; // Create a variable to store the type of bank being used

        public SBS()
        {
            try
            {
                // Define the necessary functions to make the script work as intended
                Tick += OnTick; // Runs each frame
                KeyDown += OnKeyDown; // When a key is pressed DOWN
                Aborted += OnAbort; // When the mod is closed/reloaded

                // Run everything that is necessary on startup
                VerifyDirectories(); // Verify that all configuration files are present
                Load(); // Load all accounts and the configuration
                SetupMenus(); // Set up all UI menus

                UI.Notify($@"~y~Starmans Banking System 1.1 ~w~loaded successfully!"); // Notify the user that the mod has loaded
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void VerifyDirectories()
        {
            try
            {
                if (!Directory.Exists(modDirectory)) Directory.CreateDirectory(modDirectory); // If the mod directory does not exist, create it
                if (!Directory.Exists(bankDirectory)) Directory.CreateDirectory(bankDirectory); // If the accounts directory does not exist, create it

                if (!File.Exists(modConfig)) // If there is no mod configuration
                {
                    configuration = new Configuration() // Create a new configuration and assign it to "configuration"
                    {
                        banks = new List<Bank>(), // Set banks to be a new list  of banks
                        settings = new Settings() // Set settings to be a new settings class
                        {
                            addBankKey = Keys.N, // Set the add bank key to be N
                            addBankRequiresShift = true // Make adding banks require shift to be held
                        }
                    };

                    File.WriteAllText(modConfig, JsonConvert.SerializeObject(configuration, Formatting.Indented)); // Serialize the configuration and save it
                }

                if (!File.Exists(franklinsBank)) // If there is no file for Franklins bank info
                {
                    BankAccount account = new BankAccount() // Make a new account
                    {
                        balance = 0, // Set the balance to 0
                        owner = AccountOwner.franklin, // Set the owner to Franklin
                        transactions = new List<Transaction>(), // Set transactions to a new list of transactions
                        isOpened = false, // Set the account to not be opened
                        bank = null // Set the bank to null
                    };
                    File.WriteAllText(franklinsBank, JsonConvert.SerializeObject(account, Formatting.Indented)); // Serialize and save the bank info
                }
                else franklinsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(franklinsBank)); // If the account file does exist, load it

                if (!File.Exists(michaelsBank)) // If there is no file for Michaels bank info
                {
                    BankAccount account = new BankAccount() // Make a new account
                    {
                        balance = 0, // Set the balance to 0
                        owner = AccountOwner.michael, // Set the owner to Michael
                        transactions = new List<Transaction>(), // Set transactions to a new list of transactions
                        isOpened = false, // Set the account to not be opened
                        bank = null // Set the bank to null
                    };
                    File.WriteAllText(michaelsBank, JsonConvert.SerializeObject(account, Formatting.Indented)); // Serialize and save the bank info
                }
                else michaelsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(michaelsBank)); // If the account file does exist, load it

                if (!File.Exists(trevorsBank)) // If there is no file for Trevors bank info
                {
                    BankAccount account = new BankAccount() // Make a new account
                    {
                        balance = 0, // Set the balance to 0
                        owner = AccountOwner.trevor, // Set the owner to Trevor
                        transactions = new List<Transaction>(), // Set transactions to a new list of transactions
                        isOpened = false, // Set the account to not be opened
                        bank = null // Set the bank to null
                    };
                    File.WriteAllText(trevorsBank, JsonConvert.SerializeObject(account, Formatting.Indented)); // Serialize and save the bank info
                }
                else trevorsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(trevorsBank)); // If the account file does exist, load it

                if (!File.Exists(globalBank)) // If there is no file for the global bank info
                {
                    BankAccount account = new BankAccount() // Make a new account
                    {
                        balance = 0, // Set the balance to 0
                        owner = AccountOwner.global, // Set the owner to global
                        transactions = new List<Transaction>(), // Set transactions to a new list of transactions
                        isOpened = false, // Set the account to not be opened
                        bank = null // Set the bank to null
                    };
                    File.WriteAllText(globalBank, JsonConvert.SerializeObject(account, Formatting.Indented)); // Serialize and save the bank info
                }
                else globalAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(globalBank)); // If the account file does exist, load it
            }
            catch (Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void SetupMenus()
        {
            try
            {
                // Bank Creation
                bankTypes.Add("Fleeca"); // Add fleeca to the list of bank types
                bankTypes.Add("Pacific Standard"); // Add pacific standard to the list of bank types
                bankTypes.Add("Blaine County"); // Add blaine county to the list of bank types
                bankTypes.Add("Generic"); // Add generic to the list of bank types
                bankType = new UIMenuListItem("Type:", bankTypes, 1); // Set bankType to be a UIMenuListItem listing "bankTypes"
                createBank.Activated += MenuItemPressed; // When the create bank button is pressed run MenuItemPressed()
                bankCreationMenu.AddItem(bankType); // Add bankType to the bank creation menu
                bankCreationMenu.AddItem(createBank); // Add the create bank at current position button to the bank creation menu
                pool.Add(bankCreationMenu); // Add the bank creation menu to the menu pool

                // Bank Menu
                bankAccounts.Add("Franklin"); // Add Franklin to the list of bank accounts if the account is opened
                bankAccounts.Add("Michael"); // Add Michael to the list of bank accounts if the account is opened
                bankAccounts.Add("Trevor"); // Add Trevor to the list of bank accounts if the account is opened
                bankAccounts.Add("Global"); // Add global to the list of bank accounts if the account is opened
                manageAccount = pool.AddSubMenu(bankMenu, "Manage Account"); // Add the Manage submenu to the menu pool
                manageAccount.AddItem(createAccount); // Add open account to the manage menu
                manageAccount.AddItem(closeAccount); // Add close account to the manage menu
                bankAccountList = new UIMenuListItem("Target Account:", bankAccounts, 1);
                createAccount.Activated += MenuItemPressed; // If create account is pressed run MenuItemPressed
                closeAccount.Activated += MenuItemPressed; // If close account is pressed run MenuItemPressed
                withdraw.Activated += MenuItemPressed; // If withdraw is pressed run MenuItemPressed
                deposit.Activated += MenuItemPressed; // If deposit is pressed run MenuItemPressed
                transfer.Activated += MenuItemPressed; // If transfer is pressed run MenuitemPressed
                bankMenu.AddItem(deposit); // Add the deposit button to the bank menu
                bankMenu.AddItem(withdraw); // Add the withdraw button to the bank menu
                transferMenu = pool.AddSubMenu(bankMenu, "Transfer"); // Add the transfer submenu to the menu pool
                transferMenu.AddItem(bankAccountList); // Add the list of accounts to the transfer submenu
                transferMenu.AddItem(transfer); // Add the transfer button to the transfer submenu
                bankMenu.AddItem(balance); // Add the balance item to the bank menu
                pool.Add(bankMenu); // Add bankmenu to the menu pool
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void MenuItemPressed(UIMenu sender, UIMenuItem selectedItem)
        {
            try
            {
                BankAccount currAccount; // Make a variable for the current account
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set currAccount to Trevors Account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set currAccount to Michaels Account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set currAccount to Franklins Account
                else currAccount = globalAccount; // If you're playing as a non story character set currAccount to global

                int currentAmount = 0;

                if (selectedItem.Text == createBank.Text) // If create bank at current position was pressed
                {
                    if (bankType.Items[bankType.Index].ToString() == "Fleeca") CreateBankAtPlayerPosition(BankType.fleeca); // If the bank type is Fleeca, then make a Fleeca bank at the player position
                    else if (bankType.Items[bankType.Index].ToString() == "Pacific Standard") CreateBankAtPlayerPosition(BankType.pacificStandard); // If the bank type is Pacific Standard, then make a Pacific Standard bank at the player position
                    else if (bankType.Items[bankType.Index].ToString() == "Blaine County") CreateBankAtPlayerPosition(BankType.blaineCounty); // If the bank type is Blaine County, then make a Blaine County bank at the player position
                }
                else if (selectedItem.Text == deposit.Text)
                {
                    currentAmount = GetNumberInput(); // Get user input for the amount to deposit

                    if (Game.Player.Money >= currentAmount)
                    {
                        if (currentAmount >= 1)
                        {
                            Transaction transaction = new Transaction() // Create the transaction to be logged
                            {
                                transactionAmount = currentAmount,
                                transactionDate = World.CurrentDate,
                                transactionText = "DEPOSIT ISSUED BY ACCOUNT OWNER",
                                type = TransactionType.deposit
                            };
                            Game.Player.Money = Game.Player.Money - currentAmount; // Remove the funds from the players wallet
                            currAccount.balance = currAccount.balance + currentAmount; // Add the funds to the bank
                            currAccount.transactions.Add(transaction); // Log the transaction
                            UI.Notify($"Successfully deposited ~y~${currentAmount}~w~ to your account!"); // notify the user about their transaction
                        }
                    }
                    else
                    {
                        UI.Notify("~r~Error: You lack the required funds to make this transaction"); // Notify the user that they don't have enough money
                    }
                }
                else if (selectedItem.Text == withdraw.Text)
                {
                    currentAmount = GetNumberInput(); // Get user input for the amount to withdraw

                    if (currAccount.balance >= currentAmount)
                    {
                        if (currentAmount >= 1)
                        {
                            Transaction transaction = new Transaction() // Create the transaction to be logged
                            {
                                transactionAmount = currentAmount,
                                transactionDate = World.CurrentDate,
                                transactionText = "WITHDRAWAL ISSUED BY ACCOUNT OWNER",
                                type = TransactionType.withdrawal
                            };
                            Game.Player.Money = Game.Player.Money + currentAmount; // Add the funds to the players wallet
                            currAccount.balance = currAccount.balance - currentAmount; // Remove the funds from the bank
                            currAccount.transactions.Add(transaction); // Log the transaction
                            UI.Notify($"Successfully withdrawn ~y~${currentAmount}~w~ from your account!"); // notify the user about their transaction
                        }
                    }
                    else
                    {
                        UI.Notify("~r~Error: You lack the required funds to make this transaction"); // Notify the user that they don't have enough money in their bank account
                    }
                }
                else if (selectedItem.Text == closeAccount.Text)
                {
                    if (currAccount.isOpened && currentBank.typeOfBank == currAccount.bank.typeOfBank)
                    {
                        Game.Player.Money = Game.Player.Money + currAccount.balance - currAccount.bank.startingMoney / 2; // Charge the player
                        currAccount.balance = 0; // Set the balance of the account to 0
                        currAccount.isOpened = false; // Set the account to be closed
                        currAccount.bank = null; // Set the bank the account was created with to null
                        UI.Notify($"You have successfully closed your account!"); // Notify the user that they have closed their account
                    }
                    else
                    {
                        string bankname = null; // Create a variable to store the bank name to be used in the UI Notification
                        if (currAccount.bank.typeOfBank == BankType.fleeca) bankname = "Fleeca"; // If the bank is fleeca, set bankname to fleeca
                        else if (currAccount.bank.typeOfBank == BankType.blaineCounty) bankname = "Blaine County Savings"; // If the bank is blaine county, set bankname to blaine county savings
                        else if (currAccount.bank.typeOfBank == BankType.pacificStandard) bankname = "Pacific Standard"; // If the bank is pacific standard, set bankname to pacific standard
                        UI.Notify($"~r~You cannot close your account here! go to (a) {bankname} bank to close your account"); // Tell the user they cannot close an account from a different bank
                    }
                }
                else if (selectedItem.Text == createAccount.Text)
                {
                    if (!currAccount.isOpened)
                    {
                        currAccount.bank = currentBank; // Set the accounts bank to the current bank
                        currAccount.balance = currentBank.startingMoney; // Add the starting money to the account
                        currAccount.isOpened = true; // Set the account to be opened
                        Game.Player.Money = Game.Player.Money - currentBank.startingMoney / 2; // Charge the player money for making an account
                        string bankname = null; // Create a variable to store the bank name to be used in the UI Notification
                        if (currentBank.typeOfBank == BankType.fleeca) bankname = "Fleeca"; // If the bank is fleeca, set bankname to fleeca
                        else if (currentBank.typeOfBank == BankType.blaineCounty) bankname = "Blaine County Savings"; // If the bank is blaine county, set bankname to blaine county savings
                        else if (currentBank.typeOfBank == BankType.pacificStandard) bankname = "Pacific Standard"; // If the bank is pacific standard, set bankname to pacific standard
                        UI.Notify($"You have successfully opened an account with ~y~{bankname}~w~!"); // Notify the user that they have opened an account
                    }
                }
                else if (selectedItem.Text == transfer.Text)
                {
                    int amountToTransfer = GetNumberInput(); // Create an int to store the amount to transfer and set it to the users input

                    if (currAccount.balance >= amountToTransfer)
                    {
                        if (bankAccountList.Items[bankAccountList.Index].ToString() == "Franklin" && currAccount != franklinsAccount && franklinsAccount.isOpened) franklinsAccount.balance = franklinsAccount.balance + amountToTransfer; // If the selected item is Franklins account, transfer the funds to Franklins account
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Michael" && currAccount != michaelsAccount && michaelsAccount.isOpened) michaelsAccount.balance = michaelsAccount.balance + amountToTransfer; // If the selected item is Michaels account, transfer the funds to Michaels account
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Trevor" && currAccount != trevorsAccount && trevorsAccount.isOpened) trevorsAccount.balance = trevorsAccount.balance + amountToTransfer; // If the selected item is Trevors account, transfer the funds to Trevors account
                        else if (bankAccountList.Items[bankAccountList.Index].ToString() == "Global" && currAccount != globalAccount && globalAccount.isOpened) globalAccount.balance = globalAccount.balance + amountToTransfer; // If the selected item is the Global account, transfer the funds to the Global account
                        else
                        {
                            UI.Notify("Selected account is not opened!"); // Tell the user about the account not being opened
                            return; // Return out of the function
                        }

                        currAccount.balance = currAccount.balance - amountToTransfer; // Subtract funds from the players current account

                        UI.Notify($"Successfully Transferred ~y~${amountToTransfer}~w~ to ~y~{bankAccountList.Items[bankAccountList.Index].ToString()}~w~!"); // Notify the user about their transaction
                    }
                    else
                    {
                        UI.Notify("~r~Error: You lack the required funds to make this transaction"); // Notify the user about the fact they do not have enough money to transfer
                    }
                }

                if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Saving changes");
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set Trevors account to current account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set Michaels account to current account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set Franklins to cranklins account
                else currAccount = globalAccount; // If you're playing as a non story character set the global account to current account
                Save(); // Save all changes
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void CreateBankAtPlayerPosition(BankType typeOfBank)
        {
            try
            {
                Vector3 currentPos = Game.Player.Character.Position;
                Bank newBank = new Bank() // Create a new bank
                {
                    XPos = currentPos.X, // Define the X Position of the bank
                    YPos = currentPos.Y, // Define the Y Position of the bank
                    ZPos = currentPos.Z, // Define the Z Position of the bank
                    typeOfBank = typeOfBank // Define the type of bank
                };
                if (newBank.typeOfBank == BankType.fleeca) newBank.startingMoney = 250; // If the bank is fleeca, set the starting money to 250
                else if (newBank.typeOfBank == BankType.blaineCounty) newBank.startingMoney = 50; // If the bank is blaine county set the starting money to 50
                else if (newBank.typeOfBank == BankType.pacificStandard) newBank.startingMoney = 500; // If the bank is pacific standard set the starting money to 500
                configuration.banks.Add(newBank); // Add the new bank to the bank list
                Save(); // Save the new bank
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private static void Save()
        {
            try
            {
                File.WriteAllText(michaelsBank, JsonConvert.SerializeObject(michaelsAccount, Formatting.Indented)); // Serialize and save Michaels account
                File.WriteAllText(franklinsBank, JsonConvert.SerializeObject(franklinsAccount, Formatting.Indented)); // Serialize and save Franklins account
                File.WriteAllText(trevorsBank, JsonConvert.SerializeObject(trevorsAccount, Formatting.Indented)); // Serialize and save Trevors account
                File.WriteAllText(globalBank, JsonConvert.SerializeObject(globalAccount, Formatting.Indented)); // Serialize and save the global account
                File.WriteAllText(modConfig, JsonConvert.SerializeObject(configuration, Formatting.Indented)); // Serialize and save the mod configuration
                if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Saved all accounts and the configuration");
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                michaelsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(michaelsBank)); // Deserialize Michaels account
                franklinsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(franklinsBank)); // Deserialize Franklins account
                trevorsAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(trevorsBank)); // Deserialize Trevors account
                globalAccount = JsonConvert.DeserializeObject<BankAccount>(File.ReadAllText(globalBank)); // Deserialize the global account
                configuration = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(modConfig)); // Deserialize the mod configuration

                foreach (Bank bank in configuration.banks) // Iterate over every bank in the configuration
                {
                    Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos); // Put the raw coordinates of the bank into a Vector3
                    Blip blp = World.CreateBlip(bankPos); // Create a blip at the bank position
                    blp.IsShortRange = true; // Set the blip to be short range so it is not always on the map
                    blp.Sprite = BlipSprite.Store; // Set the blip sprite to be the store icon
                    blp.Color = BlipColor.Green; // Set the blip colour to green
                    if (bank.typeOfBank == BankType.fleeca) blp.Name = "Fleeca Bank"; // If the bank is fleeca name the blip appropriately
                    else if (bank.typeOfBank == BankType.blaineCounty) blp.Name = "Blaine County Bank"; // If the bank is blaine county name the blip appropriately
                    else if (bank.typeOfBank == BankType.pacificStandard) blp.Name = "Pacific Standard Bank"; // If the bank is pacific standard name the blip appropriately
                    blips.Add(blp); // Add the blip to the list of blips to delete
                    if (configuration.settings.showDebugMessages) UI.Notify($"DEBUG: Loading Bank: {bank.typeOfBank.ToString()} at {bankPos.ToString()}");
                }
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            try
            {
                BankAccount currAccount; // Make a variable for the current account
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set currAccount to Trevors Account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set currAccount to Michaels Account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set currAccount to Franklins Account
                else currAccount = globalAccount; // If you're playing as a non story character set currAccount to global

                if (currAccount.isOpened)
                {
                    if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Enabling deposit, withdraw, balance, and open account menu items");
                    // Enable deposit, withdraw, transfer, balance, and mone amount
                    deposit.Enabled = true;
                    withdraw.Enabled = true;
                    transfer.Enabled = true;
                    balance.Enabled = true;
                    createAccount.Enabled = false; // Disable the open account button
                    closeAccount.Enabled = true; // Enable close account
                }
                else
                {
                    if (configuration.settings.showDebugMessages) UI.Notify("DEBUG: Disabling deposit, withdraw, balance, and close account menu items");
                    // Disable deposit, withdraw, transfer, balance, and mone amount
                    deposit.Enabled = false;
                    withdraw.Enabled = false;
                    transfer.Enabled = false;
                    balance.Enabled = false;
                    createAccount.Enabled = true; // Enable the open account button
                    closeAccount.Enabled = false; // Disable the close account button
                }



                balance.Text = $"Balance: ${currAccount.balance}"; // Set the balance menu items text to the balance of the account

                if (pool != null) pool.ProcessMenus(); // Process all UI Menus if the menu pool is not nulled
                foreach (Bank bank in configuration.banks)
                {
                    Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos - 1); // Put the raw coordinates of the bank into a Vector3 and remove 1 from the Z position so the vector is on the ground
                    Vector3 bankPosNM = new Vector3(bank.XPos, bank.YPos, bank.ZPos); // Put the raw coordinates of the bank into a Vector3
                    if (World.GetDistance(Game.Player.Character.Position, bankPosNM) <= .75f && !bankMenu.Visible && !manageAccount.Visible && !transferMenu.Visible) // If the player is less than .75 units away from the bank and the bank menu is not visible
                    {
                        UI.ShowSubtitle("Press ~y~E~w~ to use the bank", 1); // Notify the user they can use the bank
                    }
                    if (bankMenu.Visible || manageAccount.Visible) Game.Player.Character.FreezePosition = true; // Freeze the player if the bank menu is visible
                    else if (!bankMenu.Visible && !manageAccount.Visible) Game.Player.Character.FreezePosition = false; // Unfreeze the player if the bank menu is not visible
                    World.DrawMarker(MarkerType.VerticalCylinder, bankPos, Vector3.Zero, Vector3.Zero, new Vector3(1, 1, 1), Color.Yellow); // Draw the yellow cylinder at the bank
                }
            }
            catch (Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        private void OnAbort(object sender, EventArgs e)
        {
            foreach(Blip blip in blips) // Iterate over every blip in the blips list
            {
                blip.Remove(); // Remove the blip
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Shift) // If shift is being held (this is not with e.KeyCode since there may be multiple keys under this)
                {
                    if (e.KeyCode == Keys.N && Game.Player.Character.CurrentVehicle == null) // If N is being pressed and the player is not in a vehicle
                    {
                        bankCreationMenu.Visible = !bankCreationMenu.Visible;
                    }
                }
                if (e.KeyCode == Keys.E) // If E is pressed
                {
                    foreach (Bank bank in configuration.banks)
                    {
                        Vector3 bankPos = new Vector3(bank.XPos, bank.YPos, bank.ZPos); // Put the raw coordinates of the bank into a Vector3
                        if (World.GetDistance(Game.Player.Character.Position, bankPos) <= .75f) // If the player is less than .75 units away from the bank
                        {
                            currentBank = bank; // Set the current bank type to be the bank that the player is near
                            bankMenu.Visible = !bankMenu.Visible; // Toggle the bank menu
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
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
                    Int32.TryParse(Game.GetUserInput("", 7), out number); // Try to convert the item value to an int and store it in itemValue
                }
                catch // if the game could not parse the string
                {
                    UI.Notify("Invalid Input. Please try again"); // Notify the user of their fault
                    Wait(500); // Give the player a tiny bit of time before re opening the GetUserInput window
                    goto returnpoint; // Return to returnpoint to get input again
                }
                return number;
            }
            catch (Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
                return 0;
            }
        }

        public static void ChargePlayerAccount(int amount, string bankLogText = "PAYMENT ISSUED BY EXTERNAL SCRIPT")
        {
            try
            {
                BankAccount currAccount; // Make a variable for the current account
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set currAccount to Trevors Account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set currAccount to Michaels Account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set currAccount to Franklins Account
                else currAccount = globalAccount; // If you're playing as a non story character set currAccount to global

                if (currAccount.balance >= amount)
                {
                    Transaction transaction = new Transaction() // Create a transaction variable to log this transaction
                    {
                        transactionAmount = amount,
                        transactionDate = World.CurrentDate,
                        transactionText = bankLogText,
                        type = TransactionType.withdrawal
                    };

                    currAccount.balance = currAccount.balance - amount; // Charge the bank account

                    currAccount.transactions.Add(transaction); // Add the transaction to the logs
                }

                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set Trevors account to current account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set Michaels account to current account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set Franklins to cranklins account
                else currAccount = globalAccount; // If you're playing as a non story character set the global account to current account
                Save(); // Save all changes
            }
            catch (Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }

        public static void AddToPlayerAccount(int amountToAdd, string bankLogText = "DEPOSIT ISSUED BY EXTERNAL SCRIPT")
        {
            try
            {
                BankAccount currAccount; // Make a variable for the current account
                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set currAccount to Trevors Account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set currAccount to Michaels Account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set currAccount to Franklins Account
                else currAccount = globalAccount; // If you're playing as a non story character set currAccount to global

                Transaction transaction = new Transaction() // Create a transaction variable to log this transaction
                {
                    transactionAmount = amountToAdd,
                    transactionDate = World.CurrentDate,
                    transactionText = bankLogText,
                    type = TransactionType.deposit
                };

                currAccount.balance = currAccount.balance + amountToAdd; // Add the money to the players account

                currAccount.transactions.Add(transaction); // Add the transaction to the logs

                if (Game.Player.Character.Model == new Model("player_two")) currAccount = trevorsAccount; // If you're playing as Trevor set Trevors account to current account
                else if (Game.Player.Character.Model == new Model("player_zero")) currAccount = michaelsAccount; // If you're playing as Michael set Michaels account to current account
                else if (Game.Player.Character.Model == new Model("player_one")) currAccount = franklinsAccount; // If you're playing as Franklin set Franklins to cranklins account
                else currAccount = globalAccount; // If you're playing as a non story character set the global account to current account
                Save(); // Save all changes
            }
            catch (Exception ex)
            {
                UI.Notify($"Starmans Jobs has experienced an error: {ex.Message}");
            }
        }
    }
}
