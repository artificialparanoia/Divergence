using HarmonyLib;
using OWML.ModHelper;
using System.Linq;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace Divergence {
    [HarmonyPatch]
    public static class Patch {
        static Coroutine _removeArtificialSun = null;
        static Coroutine _updateFog = null;
        static Coroutine _tuneRiverColor = null;
        static Coroutine _changeLantern = null;
        static Coroutine _changeTextOfMainframe = null;
        static Coroutine _tuneLighting = null;
        static Coroutine _setAlarmBell = null;

        static Material _riverMat = null;
        static Material _underwaterFogMat = null;

        static DamDestructionController _damDestructionController = null;
        static DreamCampfire _dreamCampfireZone1 = null;
        static DreamCampfire _dreamCampfireZone2 = null;
        static OWRenderer _prisonerArtifactFire = null;

        public static void Initialize() {
            LoadManager.OnCompleteSceneLoad += (scene, loadScene) => {
                if (loadScene == OWScene.SolarSystem) {
                    if(_removeArtificialSun != null) {
                        Divergence.Instance.StopCoroutine(_removeArtificialSun);
                        _removeArtificialSun = null;
                    }
                    _removeArtificialSun = Divergence.Instance.StartCoroutine(RemoveArtificialSun());

                    if(_updateFog != null) {
                        Divergence.Instance.StopCoroutine(_updateFog);
                        _updateFog = null;
                    }
                    _updateFog = Divergence.Instance.StartCoroutine(UpdateFog());

                    if(_tuneRiverColor != null) {
                        Divergence.Instance.StopCoroutine(_tuneRiverColor);
                        _tuneRiverColor = null;
                    }
                    if(_riverMat != null) {
                        GameObject.Destroy(_riverMat);
                        _riverMat = null;
                    }
                    if(_underwaterFogMat != null) {
                        GameObject.Destroy(_underwaterFogMat);
                        _underwaterFogMat = null;
                    }
                    _tuneRiverColor = Divergence.Instance.StartCoroutine(TuneRiverColor());

                    if(_changeLantern != null) {
                        Divergence.Instance.StopCoroutine(_changeLantern);
                        _changeLantern = null;
                    }
                    _changeLantern = Divergence.Instance.StartCoroutine(ChangeLantern());

                    if(_changeTextOfMainframe != null) {
                        Divergence.Instance.StopCoroutine(_changeTextOfMainframe);
                        _changeTextOfMainframe = null;
                    }
                    _changeTextOfMainframe = Divergence.Instance.StartCoroutine(ChangeTextOfMainframe());

                    if(_tuneLighting != null) {
                        Divergence.Instance.StopCoroutine(_tuneLighting);
                        _tuneLighting = null;
                    }
                    _tuneLighting = Divergence.Instance.StartCoroutine(TuneLighting());

                    if (_setAlarmBell != null)
                    {
                        Divergence.Instance.StopCoroutine(_setAlarmBell);
                        _setAlarmBell = null;
                    }
                    _setAlarmBell = Divergence.Instance.StartCoroutine(SetAlarmBell());
                }
            };
        }

        static IEnumerator RemoveArtificialSun() {
            while(true) {
                yield return null;
                var ambientLightIPSurface = GameObject.Find("RingWorld_Body/Sector_RingInterior/Lights_RingInterior/AmbientLight_IP_Surface");
                if (ambientLightIPSurface) {
                    yield return null; // See https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/b6702b46a7943a50877fd8f8db4cf0a0621331ee/NewHorizons/Handlers/PlanetCreationHandler.cs#L951-L952C29
                    yield return null; // but it does not have waiting idk https://github.com/Outer-Wilds-New-Horizons/new-horizons/blob/b6702b46a7943a50877fd8f8db4cf0a0621331ee/NewHorizons/Builder/Props/DetailBuilder.cs#L194
                    ambientLightIPSurface.SetActive(false);
                    //GameObject.Destroy(ambientLightIPSurface); // simply destroying causes broken river shader idk
                    break;
                }
            }
            while(true) {
                yield return null;
                var ipSunLight = GameObject.Find("RingWorld_Body/Sector_RingInterior/Lights_RingInterior/IP_SunLight");
                if (ipSunLight) {
                    ipSunLight.SetActive(false);
                    break;
                }
            }

            while(true) {
                yield return null;
                var artificialSunBulb = GameObject.Find("Sector_RingInterior/Geometry_RingInterior/Structure_IP_ArtificialSun/ArtificialSun_Bulb");
                if (artificialSunBulb) {
                    artificialSunBulb.GetComponent<OWEmissiveRenderer>().SetEmissionColor(Color.black);
                    break;
                }
            }

            while(true) {
                yield return null;
                var hazardVolumeArtificialSun = GameObject.Find("RingWorld_Body/Sector_RingInterior/Interactibles_RingInterior/HazardVolume_ArtificialSun");
                if(hazardVolumeArtificialSun) {
                    hazardVolumeArtificialSun.GetComponent<OWTriggerVolume>().SetTriggerActivation(false);
                    break;
                }
            }
        }

        static IEnumerator TuneRiverColor() {
            while(true) {
                yield return null;
                var river = GameObject.Find("RingWorld_Body/Sector_RingInterior/Volumes_RingInterior/RingRiverFluidVolume/RingworldRiver");
                if(river) {
                    var tessellatedRingRenderer = river.GetComponent<TessellatedRingRenderer>();
                    _riverMat = new Material(tessellatedRingRenderer.sharedMaterial);
                    //_riverMat.SetColor("_Color", Color.black); // index: 1, Water Surface Color
                    _riverMat.SetColor("_FogColor", Color.black); // index: 18, Particulate Color
                    _riverMat.SetFloat("_Glossiness", 0.02f); // index: 2, Water Smoothness
                    tessellatedRingRenderer.sharedMaterial = _riverMat;
                    break;
                }
            }

            while(true) {
                yield return null;
                var fluidOxygenVolume = GameObject.Find("RingWorld_Body/Sector_RingWorld/Volumes_RingWorld/FluidOxygenVolume");
                if(fluidOxygenVolume) {
                    var effectRuleset = fluidOxygenVolume.GetComponent<EffectRuleset>();
                    if(effectRuleset) {
                        _underwaterFogMat = new Material(effectRuleset._material);
                        _underwaterFogMat.SetColor("_FogColor", new Color(0.00359f, 0.01044f, 0.0149f));
                        effectRuleset._material = _underwaterFogMat;
                        break;
                    }
                }
            }
        }

        static IEnumerator UpdateFog() {
            PlanetaryFogController fog;
            while(true) {
                yield return null;
                var fogSphere = GameObject.Find("RingWorld_Body/Atmosphere_IP/FogSphere");
                if(fogSphere) {
                    yield return null;
                    yield return null;
                    fog = fogSphere.GetComponent<PlanetaryFogController>();
                    break;
                }
            }

            while(true) {
                yield return null;
                if(!fog) {
                    yield break;
                }
                if(fog.fogDensity < 0.05f) {
                    fog.fogDensity = 0.05f;
                }
                if(fog.fogDensity > 0.37f) {
                    fog.fogDensity = 0.37f;
                }
            }
        }

        static IEnumerator ChangeLantern() {
            while(true) {
                yield return null;
                var brokenLantern = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Structures_Zone1/BrokenLanternHouse_Zone1/Props_BrokenLanternHouse_Zone1/Prefab_IP_BROKENLanternItem (1)");
                if(brokenLantern) {
                    yield return null;
                    yield return null;
                    brokenLantern.SetActive(false);
                    break;
                }
            }

            while(true) {
                yield return null;
                var originalLantern = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_DreamFireHouse_Zone1/Interactables_DreamFireHouse_Zone1/Lanterns/Prefab_IP_SimpleLanternItem_Zone1DFH_4");
                if(originalLantern) {
                    var lantern = GameObject.Instantiate(originalLantern);
                    lantern.transform.parent = GameObject.Find("RingWorld_Body").transform;
                    lantern.transform.localPosition = new Vector3(-127.7076f, 39.9843f, -254.1584f);
                    lantern.transform.localEulerAngles = new Vector3(19.7637f, 118.1841f, 90.0002f);
                    var simpleLanternItem = lantern.GetComponent<SimpleLanternItem>();
                    simpleLanternItem.enabled = true;
                    yield return null;
                    yield return null;
                    simpleLanternItem.enabled = false;
                    break;
                }
            }
        }

        static IEnumerator TuneLighting() {
            while(true) {
                yield return null;
                var spotlightOnDeck = GameObject.Find("RingWorld_Body/Sector_RingWorld/Sector_ObservationDeck/Lighting_ObservationDeck/OtherComponentsGroup/Prefab_IP_Lantern_Hanging/PointLight_Lantern_Large");
                if(spotlightOnDeck) {
                    spotlightOnDeck.transform.localPosition = new Vector3(3.655f, -4.1401f, 5.5517f);
                    var light = spotlightOnDeck.GetComponent<Light>();
                    light.intensity = 1.5f;
                    break;
                }
            }

            while(true) {
                yield return null;
                var spotlightOnJamming = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_BlightedShore/Sector_JammingControlRoom_Zone4/Lighting_JammingControlRoom_Zone4/VisibleFromFar_Lighting_JammingControlRoom_Zone4/Prefab_IP_Lantern_Wall (4)/PointLight_Lantern");
                if(spotlightOnJamming) {
                    spotlightOnJamming.transform.localPosition = new Vector3(1.5575f, -0.3676f, 0.0001f);
                    var light = spotlightOnJamming.GetComponent<Light>();
                    light.intensity = 0.5f;
                    light.range = 20;
                    light.color = new Color(0.48f, 1f, 0.986f);
                    break;
                }
            }

            Light lightEyeSymbol = null;
            while(true) {
                yield return null;
                var spotlightEyeSymbol = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/EyeTempleRuins_Zone2/Lighting_EyeTempleRuins_Zone2/SpotLight_EyeSymbol");
                if(spotlightEyeSymbol) {
                    lightEyeSymbol = spotlightEyeSymbol.GetComponent<Light>();
                    lightEyeSymbol.intensity = 1.5f;
                    lightEyeSymbol.range = 40;
                    lightEyeSymbol.color = new Color(0.48f, 1f, 0.986f);
                    break;
                }
            }

            while(true) {
                yield return null;
                var spotlightFuelNearEyeSymbol = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Structures_Zone2/EyeTempleRuins_Zone2/Lighting_EyeTempleRuins_Zone2/FillLight_FuelTorches");
                if(spotlightFuelNearEyeSymbol) {
                    spotlightFuelNearEyeSymbol.SetActive(false);
                    break;
                }
            }

            while(true) {
                yield return null;
                var spotlightLab = GameObject.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Lighting_SecretEntrance/OtherComponentsGroup/HangingLanterns/Prefab_IP_Lantern_Hanging/PointLight_Lantern_Large");
                if(spotlightLab) {
                    spotlightLab.transform.localPosition = new Vector3(0.6759f, -13.0267f, 0.9658f);
                    var light = spotlightLab.GetComponent<Light>();
                    light.intensity = 1.4f;
                    light.range = 14;
                    light.color = new Color(0.48f, 1f, 0.986f);
                    break;
                }
            }

            while (true) {
                yield return null;
                if(!lightEyeSymbol) {
                    break;
                }
                lightEyeSymbol.intensity = 1.5f;
            }
        }

        static IEnumerator SetAlarmBell()
        {
            while (true)
            {
                yield return null;
                var dreamCampfireMainframe = GameObject.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Interactibles_SecretEntrance/Experiment_3/IP_Dreamfire_Mainframe/Controller_Campfire");
                if (dreamCampfireMainframe)
                {
                    var campfireController = dreamCampfireMainframe.GetComponent<DreamCampfire>();
                    var customAlarmBell = GameObject.Find("RingWorld_Body/Sector_RingWorld/Sector_SecretEntrance/Interactibles_SecretEntrance/Experiment_3/Prefab_IP_AlarmBell_1").GetComponent<AlarmBell>();
                    var customLightController = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone3/Sector_HiddenGorge/Sector_DreamFireHouse_Zone3/Interactables_DreamFireHouse_Zone3/DreamFireChamber_DFH_Zone3/Effects_IP_AlarmBellLights").GetComponent<OWLightController>();
                    customAlarmBell._lightController = customLightController;
                    campfireController._alarmBell = customAlarmBell;
                    break;
                }
            }
        }

        static IEnumerator ChangeTextOfMainframe() {
            while(true) {
                yield return null;
                var dreamWorldBody = GameObject.Find("DreamWorld_Body/Sector_DreamWorld");
                if(!dreamWorldBody) {
                    continue;
                }
                var mainframeCDT = dreamWorldBody.GetComponentsInChildren<CharacterDialogueTree>().FirstOrDefault(x => x._characterName == "Mainframe");
                if(!mainframeCDT) {
                    continue;
                }
                var text = mainframeCDT._xmlCharacterDialogueAsset.text.Replace("<Page>SOLAR SAILS: OK</Page>", "<Page>SOLAR SAILS: OK</Page>\n<Page>DAM INTEGRITY: {{DAM_INTEGRITY}}</Page>")
                                                                       .Replace("STARLIT COVE: OK (10 occupants)", "STARLIT COVE: {{STARLIT_COVE_STATE}}")
                                                                       .Replace("SHROUDED WOODLANDS: OK (11 occupants)", "SHROUDED WOODLANDS: {{SHROUDED_WOODLANDS_STATE}}")
                                                                       .Replace("SUBTERRANEAN LAKE: OK (1 occupant)", "SUBTERRANEAN LAKE: {{SUBTERRANEAN_LAKE_STATE}}")
                                                                       .Replace("All population groups stable. There are currently 34 occupants within simulation context.", "{{SIMULATION_MODULES_STATE}}. There are currently {{SIMULATION_OCCUPANTS}} occupants within simulation context.");
                var textAsset = new TextAsset(text);
                mainframeCDT.SetTextXml(textAsset);
                break;
            }

            foreach (var key in new string[] {
                "VerifyRingworldDAM INTEGRITY: {{DAM_INTEGRITY}}",
                "VerifyRingworld_SignalDAM INTEGRITY: {{DAM_INTEGRITY}}",
            }) {
                if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                    TextTranslation.s_theTable.m_table.theTable[key] = "ダムの完全性:{{DAM_INTEGRITY}}";
                }
                else {
                    TextTranslation.s_theTable.m_table.theTable[key] = "DAM INTEGRITY: {{DAM_INTEGRITY}}";
                }
            }
            foreach (var key in new string[] {
                "VerifySimSTARLIT COVE: {{STARLIT_COVE_STATE}}",
                "VerifySim_SignalSTARLIT COVE: {{STARLIT_COVE_STATE}}",
            }) {
                if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                    TextTranslation.s_theTable.m_table.theTable[key] = "星明かりの入り江:{{STARLIT_COVE_STATE}}";
                }
                else {
                    TextTranslation.s_theTable.m_table.theTable[key] = "STARLIT COVE: {{STARLIT_COVE_STATE}}";
                }
            }
            foreach (var key in new string[] {
                "VerifySimSHROUDED WOODLANDS: {{SHROUDED_WOODLANDS_STATE}}",
                "VerifySim_SignalSHROUDED WOODLANDS: {{SHROUDED_WOODLANDS_STATE}}",
            }) {
                if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                    TextTranslation.s_theTable.m_table.theTable[key] = "覆われた森林地帯:{{SHROUDED_WOODLANDS_STATE}}";
                }
                else {
                    TextTranslation.s_theTable.m_table.theTable[key] = "SHROUDED WOODLANDS: {{SHROUDED_WOODLANDS_STATE}}";
                }
            }
            foreach (var key in new string[] {
                "VerifySimSUBTERRANEAN LAKE: {{SUBTERRANEAN_LAKE_STATE}}",
                "VerifySim_SignalSUBTERRANEAN LAKE: {{SUBTERRANEAN_LAKE_STATE}}",
            })
            {
                if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE)
                {
                    TextTranslation.s_theTable.m_table.theTable[key] = "地底湖:{{SUBTERRANEAN_LAKE_STATE}}";
                }
                else
                {
                    TextTranslation.s_theTable.m_table.theTable[key] = "SUBTERRANEAN LAKE: {{SUBTERRANEAN_LAKE_STATE}}";
                }
            }

            foreach (var key in new string[] {
                "VerifySim{{SIMULATION_MODULES_STATE}}. There are currently {{SIMULATION_OCCUPANTS}} occupants within simulation context.",
                "VerifySim_Signal{{SIMULATION_MODULES_STATE}}. There are currently {{SIMULATION_OCCUPANTS}} occupants within simulation context.",
            }) {
                if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                    TextTranslation.s_theTable.m_table.theTable[key] = "模擬現実には現在{{SIMULATION_OCCUPANTS}}人のユーザがいます。{{SIMULATION_MODULES_STATE}}";
                }
                else {
                    TextTranslation.s_theTable.m_table.theTable[key] = "{{SIMULATION_MODULES_STATE}}. There are currently {{SIMULATION_OCCUPANTS}} occupants within simulation context.";
                }
            }       

            while(true) {
                yield return null;
                var damDestructionController = GameObject.Find("RingWorld_Body/Sector_RingInterior/Geometry_RingInterior/Dam_Root");
                if(damDestructionController) {
                    _damDestructionController = damDestructionController.GetComponent<DamDestructionController>();
                    break;
                }
            }
            while(true) {
                yield return null;
                var dreamCampfireZone1 = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone1/Sector_DreamFireHouse_Zone1/Interactables_DreamFireHouse_Zone1/DreamFireChamber/Prefab_IP_DreamCampfire/Controller_Campfire");
                if(dreamCampfireZone1) {
                    _dreamCampfireZone1 = dreamCampfireZone1.GetComponent<DreamCampfire>();
                    break;
                }
            }
            while(true) {
                yield return null;
                var dreamCampfireZone2 = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone2/Sector_DreamFireLighthouse_Zone2_AnimRoot/Interactibles_DreamFireLighthouse_Zone2/DreamFireChamber/Prefab_IP_DreamCampfire/Controller_Campfire");
                if(dreamCampfireZone2) {
                    _dreamCampfireZone2 = dreamCampfireZone2.GetComponent<DreamCampfire>();
                    break;
                }
            }
            while (true)
            {
                yield return null;
                var PrisonerArtifactFire = GameObject.Find("RingWorld_Body/Sector_RingInterior/Sector_Zone4/Sector_PrisonDocks/Sector_PrisonInterior/Interactibles_PrisonInterior/Prefab_IP_Sarcophagus/Prefab_IP_SleepingMummy_v2 (PRISONER)/Mummy_IP_ArtifactAnim/ArtifactPivot/Flame");
                if (PrisonerArtifactFire)
                {
                    _prisonerArtifactFire = PrisonerArtifactFire.GetComponent<OWRenderer>();
                    break;
                }
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextTranslation), nameof(TextTranslation.Translate))]
        public static void TextTranslation_Translate_Postfix(ref string __result) {
            if(__result.Contains("{{")) {
                if(__result.Contains("{{DAM_INTEGRITY}}")) {
                    var damIntegrity = (_damDestructionController ? _damDestructionController.GetIntegrityPercent() : 100);
                    var collapsed = _damDestructionController ? _damDestructionController._collapsed : false;
                    string damState;
                    if(collapsed) {
                        if(TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            damState = $"0% [致命的な問題]";
                        }
                        else {
                            damState = $"0% [CATASTROPHIC FAILURE]";
                        }
                    }
                    else if(damIntegrity >= 99) {
                        damState = "100%";
                    }
                    else {
                        if(TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            damState = $"{(int)damIntegrity}.{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}% [損傷検出]";
                        }
                        else {
                            damState = $"{(int)damIntegrity}.{UnityEngine.Random.Range(0, 10)}{UnityEngine.Random.Range(0, 10)}% [DAMAGE DETECTED]";
                        }
                    }
                    __result = __result.Replace("{{DAM_INTEGRITY}}", damState);
                }
                else if(__result.Contains("{{STARLIT_COVE_STATE}}")) {
                    var ok = _dreamCampfireZone2 ? _dreamCampfireZone2._state == Campfire.State.LIT : true;
                    string state;
                    if(ok) {
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "OK [ユーザ数10]";
                        }
                        else {
                            state = "OK [10 occupants]";
                        }
                    }
                    else {
                        if(TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "エラー [ユーザ数0]";
                        }
                        else {
                            state = "ERROR [0 occupants]";
                        }
                    }
                    __result = __result.Replace("{{STARLIT_COVE_STATE}}", state);
                }
                else if(__result.Contains("{{SHROUDED_WOODLANDS_STATE}}")) {
                    var ok = _dreamCampfireZone1 ? _dreamCampfireZone1._state == Campfire.State.LIT : true;
                    string state;
                    if(ok) {
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "OK [ユーザ数11]";
                        }
                        else {
                            state = "OK [11 occupants]";
                        }
                    }
                    else {
                        if(TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "エラー [ユーザ数0]";
                        }
                        else {
                            state = "ERROR [0 occupants]";
                        }
                    }
                    __result = __result.Replace("{{SHROUDED_WOODLANDS_STATE}}", state);
                }
                else if (__result.Contains("{{SUBTERRANEAN_LAKE_STATE}}"))
                {
                    var ok = _prisonerArtifactFire ? _prisonerArtifactFire._gameplayActive : true;
                    string state;
                    if (ok)
                    {
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "OK [ユーザ数1]";
                        }
                        else {
                            state = "OK [1 occupant]";
                        }
                    }
                    else
                    {
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE)
                        {
                            state = "OK [ユーザ数0]";
                        }
                        else
                        {
                            state = "OK [0 occupants]";
                        }
                    }
                    __result = __result.Replace("{{SUBTERRANEAN_LAKE_STATE}}", state);
                }
                else if(__result.Contains("{{SIMULATION_OCCUPANTS}}")) {
                    string integrity;
                    string state;
                    if (_dreamCampfireZone1 && _dreamCampfireZone1._state == Campfire.State.LIT && _prisonerArtifactFire._gameplayActive == true) {
                        integrity = "34";
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "すべてのユーザグループが安定しています。";
                        }
                        else {
                            state = "All population groups stable";
                        }
                    }
                    else if (_dreamCampfireZone1 && _dreamCampfireZone1._state == Campfire.State.LIT)
                    {
                        integrity = "33";
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE)
                        {
                            state = "注意:ユーザグループ地底湖のユーザが見つかりません。";
                        }
                        else
                        {
                            state = "WARNING: No occupants found for population group Subterranean Lake";
                        }
                    }
                    else if (_dreamCampfireZone2 && _dreamCampfireZone2._state == Campfire.State.LIT && _prisonerArtifactFire._gameplayActive == true)
                    {
                        integrity = "23";
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE)
                        {
                            state = "エラー:ユーザグループ覆われた森林地帯のユーザが見つかりません。";
                        }
                        else
                        {
                            state = "ERROR: No occupants found for population group Shrouded Woodlands";
                        }
                    }
                    else if (_dreamCampfireZone2 && _dreamCampfireZone2._state == Campfire.State.LIT) {
                        integrity = "22";
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "エラー:ユーザグループ覆われた森林地帯と地底湖のユーザが見つかりません。";
                        }
                        else {
                            state = "ERROR: No occupants found for population group(s) Shrouded Woodlands and Subterranean Lake";
                        }
                    }
                    else if (_prisonerArtifactFire._gameplayActive == true) {
                        integrity = "13";
                        if(TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE) {
                            state = "エラー:ユーザグループ覆われた森林地帯と星明かりの入り江のユーザが見つかりません。";
                        }
                        else {
                            state = "ERROR: No occupants found for population group(s) Shrouded Woodlands and Starlit Cove";
                        }
                    }
                    else
                    {
                        integrity = "12";
                        if (TextTranslation.s_theTable.m_language == TextTranslation.Language.JAPANESE)
                        {
                            state = "エラー:ユーザグループ覆われた森林地帯と地底湖と星明かりの入り江のユーザが見つかりません。";
                        }
                        else
                        {
                            state = "ERROR: No occupants found for population group(s) Shrouded Woodlands, Subterranean Lake, and Starlit Cove";
                        }
                    }
                        __result = __result.Replace("{{SIMULATION_OCCUPANTS}}", integrity).Replace("{{SIMULATION_MODULES_STATE}}", state);
                }
            }
        }
    }
}
