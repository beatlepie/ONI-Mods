using System.Collections.Generic;
using UnityEngine;
using PeterHan.PLib.UI;
using static Custom_Building_Categories.GameBuildingMenuData;

namespace Custom_Building_Categories
{
    public static class CustomizeBuildingMenuScreen
    {
        /// <summary>
        /// [cuztomizationScreen] is the actual screen
        /// [alignmentScreen] is used to align the [categoryScreen] and [uncategorizedScreen] to the center, as [customizationScreen] is aligned to the right
        /// [mainScreen] contains the description, [alignmentScreen], and [instructDefaultSaveClose]
        /// [instructDefaultSaveClose] contains those three buttons aligned horizontally.
        /// </summary>
        static PDialog customizationScreen;
        static PPanel alignmentScreen;
        static PPanel mainScreen;
        static PPanel instructDefaultSaveClose;

        /// <summary>
        /// These are the variables that store the isntances of the screen, as the [PDialog.Build()] creates a new [gameObject] every time its called
        /// MUST BE INITALIZED in case they don't get initialized, which will break the code when calling [GameObject.DeleteObject()]
        /// </summary>
        static GameObject customizationScreenInstance = new GameObject();
        static GameObject addcategoryScreenInstance = new GameObject();

        /// <summary>
        /// The [PPanel] below will be inside [mainScreen] which will be inside [customizeationScreen] along with the [description]
        /// [categoryScreen] contains all the categories that will show up in the game
        /// [uncategorizedScreen] contains all the uncategorized buildings or when the category is clicked
        /// </summary>
        static PPanel categoryScreen;
        static PPanel buildingsScreen;
        static PPanel uncategorizedScreen;

        /// <summary>
        /// This is a different screen that allows people to add the screen
        /// </summary>
        static PDialog addcategoryScreen;
        static bool runOnlyOnce = true;

        /// <summary>
        /// These variable is used multiple times to reference specific, commonly used objects/values
        /// </summary>
        static PButton button;
        static PButton addCategoriesButton;
        static PButton relocateCategoryButton;
        static PButton removeCategoryButton;
        static bool relocating = false;
        static bool removing = false;
        static bool uncategorizedClicked = false;
        static int categoryIndex = 0;
        static string categorySelected;
        static string buildingSelected;
        static List<string> categorySave;
        static PLabel status = new PLabel("status");

        /// <summary>
        /// This list is used to store all the category buttons, to easily add or remove them from the options screen
        /// </summary>
        static List<PButton> buttons;

        /// <summary>
        /// Variables used to store and calling settings
        /// [categoryMenu] is defined in [OnLoad] to ensure that it only copies once
        /// </summary>
        static bool UseCustomization = false;
        public static List<PlanScreen.PlanInfo> categoryMenu = new List<PlanScreen.PlanInfo>();
        public static List<string> uncategorizedBuildings = new List<string>();

        public static void CheckBuildings()
        {
            if (System.IO.File.Exists(SAVE_FILE))
            {
                List<string> buildings = new List<string>();

                // Get all the buildings and add them to one list
                foreach (PlanScreen.PlanInfo plan in SaveFileIO.DEFAULT)
                    foreach (string building in plan.data as List<string>)
                        buildings.Add(building);

                // Remove any building that already exists
                foreach (PlanScreen.PlanInfo plan in categoryMenu)
                    foreach (string building in plan.data as List<string>)
                        buildings.Remove(building);

                // Remove any building that already exists but not used
                foreach (string building in uncategorizedBuildings)
                    buildings.Remove(building);

                // Add all the remaining buildings to the uncategorized Buildings list!
                uncategorizedBuildings.AddRange(buildings);


                foreach (PlanScreen.PlanInfo plan in categoryMenu)
                    for (int i = 0; i < (plan.data as List<string>).Count; i++)
                    {
                        BuildingDef def = Assets.GetBuildingDef((plan.data as List<string>)[i]);
                        if (def == null)
                        {
                            (plan.data as List<string>).RemoveAt(i);
                            if (i != 0)
                                i--;
                        }
                    }
            }
            else
            {
                categoryMenu = new List<PlanScreen.PlanInfo>(TUNING.BUILDINGS.PLANORDER);
                // Is [Spaced Out!] allowed for this account?
                // If the DLC is not allowed, remove the last item, which is the nuclear buildings tab!
                if (!DlcManager.IsExpansion1Active())
                    categoryMenu.RemoveAt(categoryMenu.Count - 1);
                SaveFileIO.Save();
            }
        }

        public static void OnClicked()
        {
            CheckBuildings();
            CreateScreen();
        }

        /// <summary>
        /// Initializes the variables, by redefining them whenever this is run, it prevents the game from making multiple copies of the screen
        /// This occurs when the menu is closed, then repopened without restarting the game
        /// </summary>
        private static void Initialize()
        {
            // The staticructor must have a name inside it, otherwise it returns a null [PDialog]
            // The [PPanel] is a container with all the values displayed on the [PDialog] screen, the particular [PPanel] is accessible via [PDIalog.Body]
            customizationScreen = new PDialog("customizationScreen");
            customizationScreen.Title = "Cuztomize Building Menu Options";
            customizationScreen.Body.Alignment = TextAnchor.MiddleRight;
            customizationScreen.Body.Spacing = 10;

            alignmentScreen = new PPanel("alignmentScreen");
            alignmentScreen.Spacing = 10;
            alignmentScreen.AddChild(status);

            mainScreen = new PPanel("mainScreen");
            mainScreen.Spacing = 10;
            mainScreen.Direction = PanelDirection.Horizontal;
            mainScreen.Alignment = TextAnchor.UpperCenter;
            // The above code will locate all the smaller components of the screen to the upper center of their respective location

            instructDefaultSaveClose = new PPanel("instructDefaultSaveClose");
            instructDefaultSaveClose.Direction = PanelDirection.Horizontal;
            instructDefaultSaveClose.Spacing = 50;

            categoryScreen = new PPanel("categoryScreen");
            buildingsScreen = new PPanel("buildingScreen");
            uncategorizedScreen = new PPanel("uncategorizedScreen");

            addcategoryScreen = new PDialog("addcategoryScreen");
            runOnlyOnce = true;

            // RESET ALL VALUES
            buttons = new List<PButton>();
            status.Text = "";
            uncategorizedClicked = false;
            buildingSelected = null;
            categorySelected = null;
            removing = false;
            relocating = false;
        }

        // This must be [static] so [CustomBuildingCategoriesPatch.Postfix] method can be static
        // technically, we could make an instance of this class instead or something if need be
        /// <summary>
        /// This first creates and initializes the screen.
        /// </summary>
        public static void CreateScreen()
        {
            Initialize();
            InitializeSpecialButtons();


            for (int i = 0; i < categoryMenu.Count; i++)
            {
                // Make a [PButton] with a name and text
                buttons.Add(new PButton(categoryMenu[i].category.ToString()));
                buttons[i].FlexSize = new Vector2(10, 10);
                // [button.OnClick] is a delegate that allows the function/method that gets passed to be executed
                buttons[i].OnClick = delegate(GameObject button) { CategoryButtonClicked(button.name); };
                // [HashCache.Get(int hash) OR [HashCache.Get(HashedHashedString)] converts the hashed value back to the original HashedString value
                // [HashCache.Get()] is a static method that creates a new instance of the [HashCache]
                // Debug.Log(HashedStrings.Get("HashedStringS.UI.NEWBUILDCATEGORIES." + HashCache.Get().Get(BUILDINGS.PLANORDER[i].category).ToUpper() + ".BUILDMENUTITLE"));
                // HOWEVER, the above method does not work as [HashCache] uses a different database instead of the [TUNING.BUILDINGS] class
                buttons[i].Text = categoryNames[categoryMenu[i].category.ToString()];
                // Add the button to the [PPanel] container of the [PDialog]
                categoryScreen.AddChild(buttons[i]);
            }
            // Add [addCategoriesButton] and [removeCategoryButton] button to the end 
            categoryScreen.AddChild(addCategoriesButton);
            categoryScreen.AddChild(relocateCategoryButton);
            categoryScreen.AddChild(removeCategoryButton);

            // Setup for [uncategorizedScreen]
            uncategorizedScreen.AddChild(new PLabel("Uncategorized Buildings") { Text = "Uncategorized Building" });
            foreach (string building in uncategorizedBuildings)
            {
                BuildingDef def = Assets.GetBuildingDef(building);
                uncategorizedScreen.AddChild(new PButton(building)
                {
                    Text = def.Name,
                    FlexSize = new Vector2(10, 10),
                    SpriteSize = new Vector2(40, 40),
                    Sprite = def.GetUISprite("ui", false),
                    OnClick = UncategorizedBuildingClicked
                });
            }

            // Add these in a horizontal way
            mainScreen.AddChild(categoryScreen);
            mainScreen.AddChild(buildingsScreen);
            mainScreen.AddChild(uncategorizedScreen);

            alignmentScreen.AddChild(mainScreen);

            // Add the save button and close button
            button = new PButton("Instructions");
            button.Text = "Instructions";
            button.OnClick = Instructions;
            instructDefaultSaveClose.AddChild(button);
            button = new PButton("Default");
            button.Text = "Default";
            button.OnClick = delegate 
            {
                // Define a new [PDialog]
                PDialog confirmScreen = new PDialog("confirmDialog");
                confirmScreen.Title = "Are you sure?";
                confirmScreen.Body.Spacing = 5;

                // This must be initialized otherwise it will not allow the use of the variable
                GameObject confirmScreenInstance = new GameObject();

                confirmScreen.Body.AddChild(new PLabel("confirm") { Text = "Default will return the settings to normal UNTIL RESTART without erasing the save file." });

                PPanel returnClose = new PPanel("returnClose");
                returnClose.Direction = PanelDirection.Horizontal;
                returnClose.Spacing = 20;
                // If the [Return] button is pressed, close this dialog
                returnClose.AddChild(new PButton("return")
                {
                    Text = "Return",
                    OnClick = delegate
                    {
                        confirmScreenInstance.DeleteObject();
                    }
                });
                returnClose.AddChild(new PButton("Default")
                {
                    Text = "Default",
                    OnClick = delegate
                    {
                        customizationScreenInstance.DeleteObject();
                        confirmScreenInstance.DeleteObject();
                        uncategorizedBuildings = new List<string>();
                        TUNING.BUILDINGS.PLANORDER = categoryMenu = new List<PlanScreen.PlanInfo>(SaveFileIO.DEFAULT);
                        CreateScreen();
                    }
                });
                if (System.IO.File.Exists(SAVE_FILE))
                {
                    returnClose.AddChild(new PButton("Revert")
                    {
                        Text = "Revert to Save",
                        OnClick = delegate
                        {
                            customizationScreenInstance.DeleteObject();
                            confirmScreenInstance.DeleteObject();
                            uncategorizedBuildings = new List<string>(SaveFileIO.data.uncategorizedBuildings);
                            TUNING.BUILDINGS.PLANORDER = categoryMenu = new List<PlanScreen.PlanInfo>(SaveFileIO.SAVED);
                            CreateScreen();
                        }
                    });
                }
                // If [Delete Save File] is pressed, close all buttons and delete the save file
                returnClose.AddChild(new PButton("Delete Save File")
                {
                    Text = "Delete Save File",
                    OnClick = delegate
                    {
                        customizationScreenInstance.DeleteObject();
                        confirmScreenInstance.DeleteObject();
                        System.IO.File.Delete(SAVE_FILE);
                    }
                });
                confirmScreen.Body.AddChild(returnClose);
                confirmScreenInstance = confirmScreen.Build();
                confirmScreenInstance.SetActive(true);

            };
            instructDefaultSaveClose.AddChild(button);
            button = new PButton("Save");
            button.Text = "Save";
            button.OnClick = delegate(GameObject button) { Exit(button.name); }; 
            instructDefaultSaveClose.AddChild(button);
            button = new PButton("Close");
            button.Text = "Close";
            button.OnClick = delegate (GameObject button) { Exit(button.name); };
            instructDefaultSaveClose.AddChild(button);

            // Adding the [mainScreen] in the middle of the [customizationScreen]
            customizationScreen.Body.AddChild(alignmentScreen);
            // Addign the [instructDefaultSaveClose] at the end of the [customizationScreen]
            customizationScreen.Body.AddChild(instructDefaultSaveClose);

            // Show the screen
            customizationScreenInstance = customizationScreen.Build();
            customizationScreenInstance.SetActive(true);
        }

        /// <summary>
        /// This will display the instructions of this mod
        /// </summary>
        /// <param name="Instruction">The [Instructions] button's [GameObject]</param>
        private static void Instructions(GameObject Instruction)
        {
            PDialog instructionScreen = new PDialog("instructionScreen");
            instructionScreen.Title = "Instructions!";
            instructionScreen.Body.Spacing = 5;

            instructionScreen.Body.AddChild(new PLabel("instructions")
            {
                Text = 
                "Click on the category names to select the building menu, click on the buildings to remove them from their category.\n" +
                "Click on the uncategorized buildings and then click on the categories to add them to the desired category.\n" +
                "The order of the category and buildings are as they will be shown in-game!\n" +
                "If you leave buildings in the 'Uncategorized Buildings' they will not show up in the game.\n" +
                "Currently, all custom category icons are 'Base' icons, and cannot be changed.\n" +
                "Do not use the same name as other categories or buildings. The game cannot handle duplicates!\n" +
                "Although settings can be changed mid-game, you must go to main menu and resume the game to change the settings!"
            });

            instructionScreen.Show();
        }

        /// <summary>
        /// Exit the current window in the game
        /// </summary>
        /// <param name="button">This is necessary to match this method type to the [PeterHan.PLib.UI.PUIDelegates.OnClick]</param>
        private static void Exit(string button)
        {
            // If [Save] button on [customizationScreen] is pressed
            if(button == "Save")
            {
                SaveFileIO.Save();
                customizationScreenInstance.DeleteObject();
                UseCustomization = true;

                PDialog saveScreen = new PDialog("saveScreen");
                saveScreen.Title = "Save Finished!";
                saveScreen.Body.AddChild(new PLabel("label") { Text = "Please tell me any imporvements or errors you experienced on Steam!\n'" + SAVE_FILE + "' is the location of the save file!" });
                saveScreen.Show();
            }
            // If the button's name is close!
            else
            {
                // If it was saved already, then close all dialog without checking
                if (UseCustomization)
                {
                    customizationScreenInstance.DeleteObject();
                    addcategoryScreenInstance.DeleteObject();
                }
                else
                {
                    PDialog confirmScreen = new PDialog("confirmDialog");
                    confirmScreen.Title = "Save before closing!";
                    GameObject confirmScreenInstance = new GameObject();

                    confirmScreen.Body.AddChild(new PLabel("confirm") {Text = "Closing without saving will not conserve the settings!" });

                    PPanel returnClose = new PPanel("returnClose");
                    returnClose.Direction = PanelDirection.Horizontal;
                    returnClose.Spacing = 20;
                    returnClose.AddChild(new PButton("return")
                    {
                        Text = "Return",
                        OnClick = delegate
                        {
                            confirmScreenInstance.DeleteObject();
                        }
                    });
                    returnClose.AddChild(new PButton("close")
                    {
                        Text = "Close",
                        OnClick = delegate
                        {
                            customizationScreenInstance.DeleteObject();
                            confirmScreenInstance.DeleteObject();
                            addcategoryScreenInstance.DeleteObject();
                            // Read the save file again so when it is re-opened in the same instance of the game, it can load properly!
                            SaveFileIO.Read();
                        }
                    });
                    confirmScreen.Body.AddChild(returnClose);
                    confirmScreenInstance = confirmScreen.Build();
                    confirmScreenInstance.SetActive(true);
                }
            }
        }

        // For some reason...when passing [GameObject] the variable cannot be passed to other functions during the building button usage
        // When a string is passed, the string can be used again afterwards
        /// <summary>
        /// The action that will be performed when a category button is clicked
        /// </summary>
        /// <param name="button">The [HashedString.ToString()] of the button's name is passed through this variable </param>
        private static void CategoryButtonClicked(string button)
        {
            // This was made into a separately to add the building then display the category once its been added
            if (uncategorizedClicked)
            {
                categoryIndex = 0;
                foreach (PlanScreen.PlanInfo plan in categoryMenu)
                {
                    if (plan.category.ToString() == button)
                    {
                        categorySave = plan.data as List<string>;
                        categorySave.Add(buildingSelected);
                        categorySelected = categoryNames[button];
                        categoryMenu.RemoveAt(categoryIndex);
                        categoryMenu.Insert(categoryIndex, new PlanScreen.PlanInfo(categorySelected, false, categorySave));

                        // Remove the building after it has been added
                        uncategorizedBuildings.Remove(buildingSelected);
                        break;
                    }
                    categoryIndex++;
                }
                customizationScreenInstance.DeleteObject();
                CreateScreen();
            }

            // When just clicking the category
            if (!removing && !relocating)
            {
                mainScreen.RemoveChild(buildingsScreen);
                mainScreen.RemoveChild(uncategorizedScreen);

                buildingsScreen = new PPanel("buildingScreen");
                buildingsScreen.Alignment = TextAnchor.UpperCenter;

                PLabel categoryName = new PLabel("categoryName");
                categoryName.Text = categoryNames[button];

                PGridPanel buildingsGrid = new PGridPanel("buildingsGrid");
                // This is necessary...for some reason
                buildingsGrid.AddColumn(new GridColumnSpec());
                buildingsGrid.AddColumn(new GridColumnSpec());
                buildingsGrid.AddColumn(new GridColumnSpec());
                buildingsGrid.AddColumn(new GridColumnSpec());
                buildingsGrid.AddRow(new GridRowSpec());


                int i = 0;
                categoryIndex = 0;
                foreach (PlanScreen.PlanInfo plan in categoryMenu)
                {
                    if (categoryNames[plan.category.ToString()] == categoryNames[button])
                    {
                        // [PlanScreen.PlanInfo.data] is a [gameObject] that, it must be typecasted into [List<string>]
                        foreach (string building in plan.data as List<string>)
                        {
                            BuildingDef def = Assets.GetBuildingDef(building);

                            if (!def.Deprecated && !def.DebugOnly)
                            {
                                buildingsGrid.AddChild(new PButton(building)
                                {
                                    Text = def.Name,
                                    FlexSize = new Vector2(10, 20),
                                    SpriteSize = new Vector2(40, 40),
                                    Sprite = def.GetUISprite("ui", false),
                                    OnClick = delegate (GameObject buildingButton) { BuildingButtonClicked(buildingButton, categoryIndex, button); }
                                }, new GridComponentSpec(i / 3 + 1, i % 3 + 1));
                                // Make a new row every time there is 3 items
                                // Evaluate the conditional first then add to i
                                if (i++ % 3 == 0)
                                    buildingsGrid.AddRow(new GridRowSpec());
                            }
                        }
                        break;
                    }
                    categoryIndex++;
                }
                // Add the components into the new [buildingsScreen] that will be added into [mainScreen]
                buildingsScreen.AddChild(categoryName);
                buildingsScreen.AddChild(buildingsGrid);

                // Re-add the components to the options screen
                mainScreen.AddChild(buildingsScreen);
                mainScreen.AddChild(uncategorizedScreen);
            }
            else if (relocating){
                if (categorySelected != null && categorySelected != "")
                {
                    // If the exact same categories are selected, don't do anything and [return]
                    if(categorySelected == button)
                    {
                        customizationScreenInstance.DeleteObject();
                        CreateScreen();
                        return;
                    }

                    for (int  i = 0; i < categoryMenu.Count; i++)
                    {
                        if (categoryMenu[i].category.ToString() == categorySelected)
                        {
                            categoryMenu.RemoveAt(i);
                            // If the selected category is the first category do not subtract, as the adding code is executed next anyways
                            if(i != 0)
                                i--;
                            // If the selected category is the last category and swap location is second last place, just break the loop so it does not re-add it
                            if (i == categoryMenu.Count)
                                break;
                        }
                        if (categoryMenu[i].category.ToString() == button)
                        {
                            categoryMenu.Insert(i, new PlanScreen.PlanInfo(categoryNames[categorySelected], false, categorySave));
                            i++;
                        }
                    }
                    customizationScreenInstance.DeleteObject();
                    CreateScreen();
                }
                // Save the category to change, 1st click after relocating == true
                else
                {
                    categorySelected = button;
                    categoryIndex = 0;
                    status.Text = "Click on another category, the previous category will be placed above the category you click! \nClick the 'Add Category...' to add it at the end.\nClick 'Swap Order' to cancel.";
                    foreach (PlanScreen.PlanInfo plan in categoryMenu)
                    {
                        if (plan.category.ToString() == button)
                        {
                            CustomizeBuildingMenuScreen.button = buttons[categoryIndex];
                            categorySave = plan.data as List<string>;
                            break;
                        }
                        categoryIndex++;
                    }
                }
            }
            // if(removing == true)
            else
            {
                status.Text = "";
                removing = false;
                int i = 0;
                foreach (PlanScreen.PlanInfo plan in categoryMenu)
                {
                    if (plan.category.ToString() == button)
                    {
                        // [PlanScreen.PlanInfo.data] is a [gameObject] that, it must be typecasted into [List<string>]
                        foreach (string building in plan.data as List<string>)
                        {
                            if (building != "DevGenerator" && building != "SteamTurbine")
                            {
                                BuildingDef def = Assets.GetBuildingDef(building);
                                uncategorizedBuildings.Add(building);
                                uncategorizedScreen.AddChild(new PButton(building)
                                {
                                    Text = def.Name,
                                    FlexSize = new Vector2(10, 10),
                                    SpriteSize = new Vector2(40, 40),
                                    Sprite = def.GetUISprite("ui", false)
                                });
                            }
                        }
                        categoryMenu.Remove(plan);
                        break;
                    }
                    i++;
                }
                categoryScreen.RemoveChild(buttons[i]);
            }
            // Update and remake the screen
            customizationScreenInstance.DeleteObject();
            customizationScreenInstance = customizationScreen.Build();
            customizationScreenInstance.SetActive(true);
        }

        /// <summary>
        /// Remove the building from the category they were in and add it to the uncategorized section
        /// Runs the [CategoryButtonClicked] function to redraw the [customizationScreen]
        /// </summary>
        /// <param name="building">The [GameObject] of the button clicked</param>
        /// <param name="categoryIndex">The index of the category the building is stored in</param>
        /// <param name="category">The [HashedString.ToString()] name of the category to update</param>
        private static void BuildingButtonClicked(GameObject building, int categoryIndex, string category)
        {
            if (relocating || uncategorizedClicked)
            {
                if (buildingSelected != null && buildingSelected != "")
                {
                    status.Text = "";
                    categorySave = new List<string>();
                    List<string> current = (categoryMenu[categoryIndex].data as List<string>);

                    // Remove the selected building, then remake add them
                    (categoryMenu[categoryIndex].data as List<string>).Remove(buildingSelected);
                    for (int i = 0; i < (categoryMenu[categoryIndex].data as List<string>).Count; i++)
                    {
                        if ((categoryMenu[categoryIndex].data as List<string>)[i] == building.name)
                        {
                            (categoryMenu[categoryIndex].data as List<string>).Insert(i, buildingSelected);
                            break;
                        }
                    }

                    if (uncategorizedClicked)
                        uncategorizedBuildings.Remove(buildingSelected);
                    relocating = false;
                    uncategorizedClicked = false;
                    buildingSelected = null;

                    // We delete and remake the entire screen to update the uncategorized building category
                    // This must happen here because [CreateScreen] does not leave [buildingScreen] on the dialog
                    customizationScreenInstance.DeleteObject();
                    CreateScreen();
                    CategoryButtonClicked(category);
                }
                else
                {
                    status.Text = "Click on a another building! \nThe previous building will be located before it. \nRemove the building and add it to the category to add it to the end.";
                    buildingSelected = building.name;
                    categorySelected = null;

                    customizationScreenInstance.DeleteObject();
                    customizationScreenInstance = customizationScreen.Build();
                    customizationScreenInstance.SetActive(true);
                }
            }
            else {
                // Add the removed building to the uncategorized screen
                uncategorizedBuildings.Add(building.name);

                // Run the [CategoryButtonClicked] again to update the screen
                (categoryMenu[categoryIndex].data as List<string>).Remove(building.name);

                // We delete and remake the entire screen to update the uncategorized building category
                // This must happen here because [CreateScreen] does not leave [buildingScreen] on the dialog
                customizationScreenInstance.DeleteObject();
                CreateScreen();
                CategoryButtonClicked(category);
            }
        }

        /// <summary>
        /// Action to be taken when the a building in the uncategorized building list is clicked
        /// </summary>
        /// <param name="building">The [GameObject] of the ucategorized building button that was clicked</param>
        private static void UncategorizedBuildingClicked(GameObject building)
        {
            if (buildingSelected == null || buildingSelected == "")
            {
                buildingSelected = building.name;
                uncategorizedClicked = true;
            }
            else if (building.name == buildingSelected)
            {
                uncategorizedClicked = false;
                buildingSelected = null;
            }

            if (uncategorizedClicked)
                status.Text = "Click on a category to add the building at the end.\nClick a building to add the building before it.\nClick again to cancel.";
            else
                status.Text = "";

            relocating = false;

            // Update and remake the screen to display the status text!
            customizationScreenInstance.DeleteObject();
            customizationScreenInstance = customizationScreen.Build();
            customizationScreenInstance.SetActive(true);
        }

        /// <summary>
        /// This class was made to divide the code and debug it more efficiently
        /// </summary>
        private static void InitializeSpecialButtons()
        {
            addCategoriesButton = new PButton("Add");
            addCategoriesButton.Text = "Add Categories...";
            addCategoriesButton.FlexSize = new Vector2(20, 40);
            addCategoriesButton.TextStyle = PUITuning.Fonts.UILightStyle;
            addCategoriesButton.TextStyle.fontSize = 18;
            addCategoriesButton.OnClick = delegate
            {
                if (relocating && categorySelected != null && categorySelected != "")
                {
                    relocating = false;
                    status.Text = "";

                    categoryMenu.Add(categoryMenu[categoryIndex]);
                    categoryMenu.RemoveAt(categoryIndex);

                    categorySelected = null;

                    // Update and remake the screen to display the status text!
                    customizationScreenInstance.DeleteObject();
                    CreateScreen();
                }
                else
                {
                    PTextArea addcategory = new PTextArea("addcategory");
                    addcategory.Text = "Type the name of the\ncategory you would\nlike to add\nCANNOT HAVE\nDUPLICATE NAME";
                    addcategory.LineCount = 5;
                    string categoryName = "";
                    addcategory.OnTextChanged = delegate (GameObject source, string text) { categoryName = text; };

                    button = new PButton("Save")
                    {
                        Text = "Save",
                        OnClick = delegate
                        {
                            // Deactivate the [addcategoryScreenInstance]
                            addcategoryScreenInstance.SetActive(false);

                            // Add the [categoryName] to the dictionary
                            // Add the new category to the list of categories
                            categoryMenu.Add(new PlanScreen.PlanInfo(categoryName, false, new List<string>()));
                            // Add/Link the [categoryName] to the Sprite ID
                            // The if statement was added in case someone tries to add something that already exists, after erasing it from the category menu...
                            if (!iconNameMap.ContainsKey(categoryName))
                            {
                                categoryNames.Add(new HashedString(categoryName).ToString(), categoryName);
                                iconNameMap.Add(HashCache.Get().Add(categoryName), "icon_category_base");
                                // Add/Link the [categoryName] ID to the [categoryName] so that the game code can call it properly
                                Strings.Add("STRINGS.UI.BUILDCATEGORIES." + categoryName.ToUpper() + ".NAME", categoryName);
                                Strings.Add("STRINGS.UI.BUILDCATEGORIES." + categoryName.ToUpper() + ".TOOLTIP", "Customized Building Category!");
                            }

                            // Update and remake the screen to display the status text!
                            customizationScreenInstance.DeleteObject();
                            CreateScreen();
                        }
                    };

                    // Only add these buttons and settings if [addcategoryScreenInstance] hasn't been defined yet
                    if (runOnlyOnce)
                    {
                        addcategoryScreen.Size = new Vector2(200, 150);
                        addcategoryScreen.Body.Spacing = 10;
                        addcategoryScreen.Title = "Add Category...";
                        addcategoryScreen.Body.AddChild(addcategory);
                        addcategoryScreen.Body.AddChild(button);
                        runOnlyOnce = false;
                    }
                    // Define the [addcategoryScreenInstance] and show it on screen
                    addcategoryScreenInstance = addcategoryScreen.Build();
                    addcategoryScreenInstance.SetActive(true);
                }
            };

            relocateCategoryButton = new PButton("relocateCategoryButton");
            relocateCategoryButton.Text = "Swap Order";
            relocateCategoryButton.FlexSize = new Vector2(20, 40);
            relocateCategoryButton.OnClick = delegate 
            {
                relocating = relocating ? false : true;
                if (relocating)
                    status.Text = "Click on the category/building to select! \nClick again to cancel";
                else
                    status.Text = "";

                // Update and remake the screen to display the status text!
                customizationScreenInstance.DeleteObject();
                customizationScreenInstance = customizationScreen.Build();
                customizationScreenInstance.SetActive(true);
            };

            removeCategoryButton = new PButton("remove")
            {
                Text = "Remove Category...",
                FlexSize = new Vector2(20, 40),
                OnClick = delegate 
                {
                    removing = removing ? false : true;
                    if (removing)
                        status.Text = "Click the category you would like to remove! \nClick again to cancel";
                    else
                        status.Text = "";

                    uncategorizedClicked = false;
                    relocating = false;
                    // Update and remake the screen to display the status text!
                    customizationScreenInstance.DeleteObject();
                    customizationScreenInstance = customizationScreen.Build();
                    customizationScreenInstance.SetActive(true);
                }
            };
        }
    }
}