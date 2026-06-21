using Ex.Kingmaker.View.MapObjects;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MyOwlcatModification.Assets.Editor.Utility
{
    [InitializeOnLoad]
    static internal class ContextMenuButtons
    {
        static Action<GameObject, GameObject, bool> GOCreationCommands_Place = AccessTools.Method(AccessTools.TypeByName("GOCreationCommands"), "Place").CreateDelegate(typeof(Action<GameObject, GameObject, bool>)) as Action<GameObject, GameObject, bool>;

        static ContextMenuButtons()
        {
            ObjectFactory.componentWasAdded += onComponentAdded;
        }

        static void onComponentAdded(Component component)
        {
            if (!Application.isEditor || !(component is Kingmaker.View.EntityViewBase view) || view is null)
                return;

            view.UniqueId = Guid.NewGuid().ToString();
        }

        [MenuItem("GameObject/Owlcat Views/Area Effect View", true)]
        static bool ValidateCreationAreaEffectView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return  !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Area Effect View", false, 10)]
        static void PerformCreationAreaEffectView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("AreaEffectView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<AreaEffectView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }



        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Anchor View", true)]
        static bool ValidateCutsceneAnchorView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Anchor View", false)]
        static void CutsceneAnchorView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CutsceneAnchorView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.AreaLogic.Cutscenes.CutsceneAnchorView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Art Controller", true)]
        static bool ValidateCutsceneArtController(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Art Controller", false)]
        static void CutsceneArtController(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CutsceneArtController");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.AreaLogic.Cutscenes.Commands.Camera.CutsceneArtController>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }



        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Path", true)]
        static bool ValidateCutscenePath(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Path", false)]
        static void CutscenePath(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CutscenePath");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.AreaLogic.Cutscenes.Commands.CutscenePath>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }



        [MenuItem("GameObject/Owlcat Views/Cutscenes/CutscenePlayerView", true)]
        static bool ValidateCutscenePlayerView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Cutscenes/Cutscene Player View", false)]
        static void CutscenePlayerView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CutscenePlayerView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.AreaLogic.Cutscenes.CutscenePlayerView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Cutscenes/Director Adapter", true)]
        static bool ValidateDirectorAdapter(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Cutscenes/Director Adapter", false)]
        static void DirectorAdapter(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("DirectorAdapter");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.AreaLogic.Cutscenes.Commands.Timeline.DirectorAdapter>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Fow Revealer Trigger", true)]
        static bool ValidateFowRevealerTrigger(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Fow Revealer Trigger", false)]
        static void FowRevealerTrigger(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("FowRevealerTrigger");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.FowRevealerTrigger>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Locator View", true)]
        static bool ValidateLocatorView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Locator View", false)]
        static void LocatorView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("LocatorView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.LocatorView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Camp Place View", true)]
        static bool ValidateCampPlaceView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Camp Place View", false)]
        static void CampPlaceView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CampPlaceView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.CampPlaceView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Dropped Loot", true)]
        static bool ValidateDroppedLoot(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Dropped Loot", false)]
        static void DroppedLoot(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("DroppedLoot");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.DroppedLoot>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Dynamic Map Object View", true)]
        static bool ValidateDynamicMapObjectView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Dynamic Map Object View", false)]
        static void DynamicMapObjectView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("DynamicMapObjectView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.DynamicMapObjectView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Script Zone", true)]
        static bool ValidateScriptZone(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Script Zone", false)]
        static void ScriptZone(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("ScriptZone");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.SriptZones.ScriptZone>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Traps/Detailed Trap Object View", true)]
        static bool ValidateDetailedTrapObjectView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Traps/Detailed Trap Object View", false)]
        static void DetailedTrapObjectView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("Detailed Trap Object View");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.Traps.Detailed.DetailedTrapObjectView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Map Objects/Traps/Simple Trap Object View", true)]
        static bool ValidateSimpleTrapObjectView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Map Objects/Traps/Simple Trap Object View", false)]
        static void SimpleTrapObjectView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("SimpleTrapObjectView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.MapObjects.Traps.Simple.SimpleTrapObjectView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Region Event Location", true)]
        static bool ValidateRegionEventLocation(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Region Event Location", false)]
        static void RegionEventLocation(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("RegionEventLocation");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.Kingdom.RegionEventLocation>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Roaming Waypoint View", true)]
        static bool ValidateRoamingWaypointView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Roaming Waypoint View", false)]
        static void RoamingWaypointView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("RoamingWaypointView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.Roaming.RoamingWaypointView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Units/Unit Entity View", true)]
        static bool ValidateUnitEntityView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Units/Unit Entity View", false, 2)]
        static void UnitEntityView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("UnitEntityView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.UnitEntityView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Units/Unit Group View", true)]
        static bool ValidateUnitGroupView(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Units/Unit Group View", false, 3)]
        static void UnitGroupView(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("UnitGroupView");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.Spawners.UnitGroupView>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Units/Companion Spawner", true)]
        static bool ValidateCompanionSpawner(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Units/Companion Spawner", false)]
        static void CompanionSpawner(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("CompanionSpawner");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.Spawners.CompanionSpawner>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




        [MenuItem("GameObject/Owlcat Views/Units/Unit Spawner", true)]
        static bool ValidateUnitSpawner(MenuCommand command)
        {
            var obj = command.context as GameObject;
            return !obj || !obj.GetComponents<Kingmaker.View.EntityViewBase>().Any();
        }

        [MenuItem("GameObject/Owlcat Views/Units/Unit Spawner", false, 1)]
        static void UnitSpawner(MenuCommand command)
        {
            GameObject gameObject = ObjectFactory.CreateGameObject("UnitSpawner");
            gameObject.SetActive(false);
            ObjectFactory.AddComponent<Ex.Kingmaker.View.Spawners.UnitSpawner>(gameObject);
            gameObject.SetActive(true);
            GOCreationCommands_Place.Invoke(gameObject, command.context as GameObject, true);
        }




    }
}
