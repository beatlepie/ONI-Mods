
namespace testing
{
    //static makes this class non-instanciable
    //this means that this class cannot have functions
    //to acess data inside this class, it must be done this way:
    //[testingModInfo.Version]
    public static class testingModInfo
    {
        public static string Name = "State Change Pipes";
        public static int Version = 5;
        public static string Date = "2020/01/10";
        //Version 1 result:
        //MOD does not work; breaks immidiately
        //Version 1 had error: 
        //NullReferenceException: Object reference not set to an instance of an object
        //at CodexEntryGenerator.GenerateBuildingEntries() [0x00000] in <filename unknown>:0 
        //at CodexCache.Init() [0x00000] in <filename unknown>:0 
        //at ManagementMenu.OnPrefabInit() [0x00000] in <filename unknown>:0 
        //at KMonoBehaviour.InitializeComponent() [0x00000] in <filename unknown>:0 
        //Rethrow as Exception: Error in TopRightInfoPanelButtons.ManagementMenu.OnPrefabInit
        //at KMonoBehaviour.InitializeComponent()[0x00000] in <filename unknown>:0 
        //at KMonoBehaviour.Awake() [0x00000] in <filename unknown>:0
        //Version 1 solution:
        //This was fixed by removing the [abstract] keyword from the MOD building classes

        //Version 2 result:
        //The game started properly, however the construction did not allow the building to connect with pipes
        //the building visuals is weird when are dragging it around without placing it...
        //Version 2 had error:
        //[22:51:24.788] [1] [ERROR] EvaporationPipeComplete UnityEngine.GameObject 'EvaporationPipeComplete' requires a component of type Operational as requested by ConduitConsumer!
        //Version 2 solution:
        //changed [Conduit] to check whether the building is MOD building which should not take damage from state change!
        //added [Operational operational = go.AddOrGet<Operational>();] which added the operational required by conduit consumer!
        //seems like conduit consumer requires ConduitType for input!
        //changed values to radiant conduit values, because not enough heat trasfer!

        //Versin 3 result:
        //There were errors that seemingly resulted due to conduit interactions. Same issue as Version 2.
        //Version 3 soltuion:
        //Originally thought of taking data inside the function and changing it, but found it extremely hard and currently not possible.
        //Will be changing the MOD from pipes to backgrond buildings.
        //By doing so, I won't have to deal with conduit connection functions :)

        //Version 4 result:
        //liquid pipes could be built, but not gas pipes, not sure why
        //^this was due to the [Objectlayer] being in gas pipes layer instead of [backwall].
        //Currently the steam just leaves the building itself... (2020/01/02)
        //^Ice Machine changes the state of material inside, thought [EvaporationPipe] must be what I need
        //Entire MOD does not load currently... (2020/01/02)
        /*
        ReflectionTypeLoadException: The classes in the module cannot be loaded.
            at (wrapper managed-to-native) System.Reflection.Assembly:GetTypes (bool)
            at System.Reflection.Assembly.GetTypes () [0x00000] in <filename unknown>:0 
            at AsyncLoadManager`1[IGlobalAsyncLoader].Run () [0x00000] in <filename unknown>:0 
            at Assets.OnPrefabInit () [0x00000] in <filename unknown>:0 
            at KMonoBehaviour.InitializeComponent () [0x00000] in <filename unknown>:0 
        Rethrow as Exception: Error in GameAssets(Clone).Assets.OnPrefabInit
            at KMonoBehaviour.InitializeComponent () [0x00000] in <filename unknown>:0 
            at KMonoBehaviour.Awake () [0x00000] in <filename unknown>:0 
            UnityEngine.Object:Internal_InstantiateSingleWithParent_Injected(Object, Transform, Vector3&, Quaternion&)
            UnityEngine.Object:Internal_InstantiateSingleWithParent(Object, Transform, Vector3, Quaternion)
            UnityEngine.Object:Instantiate(Object, Vector3, Quaternion, Transform)
            UnityEngine.Object:Instantiate(GameObject, Vector3, Quaternion, Transform)
            Util:KInstantiate(GameObject, Vector3, Quaternion, GameObject, String, Boolean, Int32)
            Util:KInstantiate(GameObject, GameObject, String)
            LaunchInitializer:Update()
        */
        //This error occurs IF .NET FrameWork is not 3.5. ONI uses .NET FrameWork of 3.5!!!!!
        //Version 4 solution:
        //I believe I found everything I need to make this MOD work. 
        //I found a comment that said aquatuner that does this, which makes more sense!

        //Version 5 result:
        //I will attempt to implement this so that it can run similar to an aquatuner.
        //The MOD is currently running almost as intended. 
        //Used [stateMachine] to make the building switch between active and inactive.
        //The MOD does not have any "smoothing" animation, but I don't think those are necessary :p
    }
}
 