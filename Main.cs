using ModLoader;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace FlightInfo
{
    public class Main : SFSMod
    {
        public GameObject GO;
        public bool gene, surf;

        public Main() : base(
           "FlightInfo", // Mod ID
           "FlightInfo", // Mod Name
           "jsudo", // Mod Author
           "v1.1.x", // Mod loader version
           "v1.1" // Mod version
           )
        {
        }

        // This initializes the patcher. This is required if you use any Harmony patches.
        public static Harmony patcher;

        public override void load()
        {
            GO = new GameObject("FlightMenu");
            Object.Destroy(GO);
        }

        public override void early_load()
        {
            // This method runs before anything from the game is loaded. This is where you should apply your patches, as shown below.

            // The patcher uses an ID formatted like a web domain.
            // Main.patcher = new Harmony("example.foo.bar");

            // This pulls your Harmony patches from everywhere in the namespace and applies them.
            // Main.patcher.PatchAll();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void unload()
        {
            // This method runs if your mod gets unloaded. This shouldn't happen, so it throws an error.
            GO = new GameObject("FlightMenu");
            Object.Destroy(GO);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "World_PC")
            {
                /*
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "config.txt";
                string[] settings = File.ReadAllLines(path);
                foreach (string setting in settings)
                {
                    //drag = (setting == "T");
                    if (setting == "1")
                        gene = true;
                    else
                        gene = false;

                    if (setting == "2")
                        surf = true;
                    else
                        surf = false;
                    // 3
                    // etc
                
                }
                */
                GO = new GameObject("FlightMenu");
                GO.AddComponent<Menu>();
                Object.DontDestroyOnLoad(GO);
                GO.SetActive(true);
            }
            else
            {
                Object.Destroy(GO);
            }
        }

        /*
        public override void load()
        {
            GO = new GameObject("Flight Stats Menu");

            // Add your new class as a component of your new GameObject so your script becomes active.
            GO.AddComponent<Menu>();

            // Make the object active.
            GO.SetActive(true);

            // Tell Unity not to destroy your GameObject when a new scene loads.
            UnityEngine.Object.DontDestroyOnLoad(GO);

        }
        */
    }
}