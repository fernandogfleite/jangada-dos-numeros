using System.Collections.Generic;
using JangadaDosNumeros.Core;
using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    [DisallowMultipleComponent]
    public sealed class VerticalSliceController : MonoBehaviour
    {
        private const float CargoReach = 1.35f;
        private const float RaftReach = 1.55f;
        private readonly RaftLoadout loadout = new RaftLoadout(8);
        private readonly GameObject[] placedVisuals = new GameObject[8];
        private readonly GameplaySession gameplay = new GameplaySession();
        private MissionDefinition mission;
        private CargoType? selectedCargo;
        private GameObject carriedVisual;
        private string feedback = "Escolha uma carga no ateliê.";
        private Transform world;
        private PlayerCharacterController character;
        private RaftNavigationView raft;
        private DeliveryZoneView deliveryZone;
        private CargoPrototypeView[] prototypes;
        private SlotView[] slots;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle feedbackStyle;
        private int cachedStyleSize;

        public void RebuildBlockout()
        {
            BlockoutFactory.BuildWorld(transform);
        }

        private void Awake()
        {
            mission = new MissionDefinition(new Dictionary<CargoType, int>
            {
                { CargoType.Peixe, 2 },
                { CargoType.Coco, 3 }
            });
            RebuildBlockout();
            CacheWorldReferences();
            ResetMission();
        }

        private void CacheWorldReferences()
        {
            world = transform.Find("World_Blockout");
            character = world.GetComponentInChildren<PlayerCharacterController>();
            raft = world.GetComponentInChildren<RaftNavigationView>();
            deliveryZone = world.GetComponentInChildren<DeliveryZoneView>();
            prototypes = world.GetComponentsInChildren<CargoPrototypeView>();
            slots = world.GetComponentsInChildren<SlotView>();
            System.Array.Sort(slots, (left, right) => left.Index.CompareTo(right.Index));
        }

        private void Update()
        {
            if (gameplay.Phase == GameplayPhase.Delivered)
            {
                return;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (gameplay.CharacterIsControlled)
            {
                character.Move(input, Time.deltaTime);
            }
            else if (gameplay.RaftIsControlled)
            {
                raft.Navigate(input, Time.deltaTime, IsRaftInDeliveryZone());
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                HandleContextAction();
            }
            HandlePointer();
        }

        private void HandlePointer()
        {
            Vector2 pointerPosition;
            if (!TryGetPointerRelease(out pointerPosition) || IsOverInterface(pointerPosition))
            {
                return;
            }

            var mainCamera = Camera.main;
            RaycastHit hit;
            if (mainCamera == null || !Physics.Raycast(mainCamera.ScreenPointToRay(pointerPosition), out hit, 100f))
            {
                return;
            }

            var prototype = hit.collider.GetComponentInParent<CargoPrototypeView>();
            if (prototype != null)
            {
                if (!gameplay.CharacterIsControlled)
                {
                    feedback = "A carga já está em viagem na jangada.";
                    return;
                }
                SelectCargo(prototype.CargoType, "Carga selecionada: " + DisplayName(prototype.CargoType) + ". Leve-a à jangada ou toque em um slot.");
                return;
            }

            var slot = hit.collider.GetComponentInParent<SlotView>();
            if (slot != null)
            {
                HandleSlot(slot);
                return;
            }

            if (hit.collider.GetComponentInParent<DestinationView>() != null)
            {
                feedback = gameplay.Phase == GameplayPhase.Sailing
                    ? "Leve a jangada até o círculo verde e pressione Espaço."
                    : "Primeiro valide a carga e navegue com a jangada até a feira.";
            }
        }

        private void HandleContextAction()
        {
            if (gameplay.Phase == GameplayPhase.Sailing)
            {
                if (!gameplay.TryDeliver(IsRaftInDeliveryZone()))
                {
                    feedback = "Navegue até o círculo verde da feira para entregar.";
                    return;
                }
                raft.Navigate(Vector2.zero, 0f, true);
                feedback = "Entrega concluída na feira! Missão completa.";
                return;
            }

            if (gameplay.Phase == GameplayPhase.ReadyToSail && !selectedCargo.HasValue && character.IsNear(raft.BoardingPoint.position, RaftReach))
            {
                if (gameplay.TryBoard(true))
                {
                    character.Board(raft.DriverAnchor);
                    feedback = "Jangada em movimento: ↑/↓ avançam e recuam; ←/→ giram.";
                }
                return;
            }

            if (selectedCargo.HasValue)
            {
                if (!character.IsNear(raft.LoadPoint.position, RaftReach))
                {
                    feedback = "Leve " + DisplayName(selectedCargo.Value) + " até a jangada.";
                    return;
                }
                var freeSlot = FirstFreeSlot();
                if (freeSlot == null)
                {
                    feedback = "A jangada está sem slots livres; a carga continua na mão.";
                    return;
                }
                HandleSlot(freeSlot);
                return;
            }

            var nearbyPrototype = FindNearestPrototype();
            if (nearbyPrototype != null)
            {
                SelectCargo(nearbyPrototype.CargoType, DisplayName(nearbyPrototype.CargoType) + " na mão. Leve até a jangada.");
                return;
            }

            var nearbyOccupiedSlot = FindNearestOccupiedSlot();
            if (nearbyOccupiedSlot != null)
            {
                HandleSlot(nearbyOccupiedSlot);
                return;
            }

            feedback = gameplay.Phase == GameplayPhase.ReadyToSail
                ? "Aproxime-se do ponto de embarque da jangada e pressione Espaço."
                : "Aproxime-se de uma carga ou da jangada para usar Espaço.";
        }

        private CargoPrototypeView FindNearestPrototype()
        {
            CargoPrototypeView nearest = null;
            var nearestDistance = CargoReach * CargoReach;
            for (var i = 0; i < prototypes.Length; i++)
            {
                var delta = prototypes[i].transform.position - character.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= nearestDistance)
                {
                    nearest = prototypes[i];
                    nearestDistance = delta.sqrMagnitude;
                }
            }
            return nearest;
        }

        private SlotView FindNearestOccupiedSlot()
        {
            SlotView nearest = null;
            var nearestDistance = RaftReach * RaftReach;
            for (var i = 0; i < slots.Length; i++)
            {
                if (!loadout.GetCargo(slots[i].Index).HasValue)
                {
                    continue;
                }
                var delta = slots[i].CargoAnchor.position - character.transform.position;
                delta.y = 0f;
                if (delta.sqrMagnitude <= nearestDistance)
                {
                    nearest = slots[i];
                    nearestDistance = delta.sqrMagnitude;
                }
            }
            return nearest;
        }

        private SlotView FirstFreeSlot()
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (!loadout.GetCargo(slots[i].Index).HasValue)
                {
                    return slots[i];
                }
            }
            return null;
        }

        private void SelectCargo(CargoType cargo, string message)
        {
            selectedCargo = cargo;
            gameplay.RevokeValidation();
            RefreshCarriedVisual();
            feedback = message;
        }

        private void HandleSlot(SlotView slot)
        {
            if (!gameplay.CharacterIsControlled)
            {
                feedback = "Os itens permanecem presos à jangada durante a viagem.";
                return;
            }

            var selectedBefore = selectedCargo;
            var action = SlotInteraction.Apply(loadout, slot.Index, ref selectedCargo);
            if (action == SlotInteractionAction.TookExisting)
            {
                DestroyPlacedVisual(slot.Index);
                gameplay.RevokeValidation();
                RefreshCarriedVisual();
                feedback = DisplayName(selectedCargo.Value) + " retirada. Leve-a ou escolha outro slot.";
                return;
            }
            if (action == SlotInteractionAction.RejectedOccupied)
            {
                feedback = "Slot ocupado. " + DisplayName(selectedCargo.Value) + " continua na mão; escolha um slot livre.";
                return;
            }
            if (action == SlotInteractionAction.NoSelection)
            {
                feedback = "Pegue uma carga antes de usar este slot.";
                return;
            }

            placedVisuals[slot.Index] = BlockoutFactory.CreateCargoObject(selectedBefore.Value,
                "Carga_Colocada_" + (slot.Index + 1).ToString("00"), slot.CargoAnchor, false);
            gameplay.RevokeValidation();
            RefreshCarriedVisual();
            feedback = "Carga colocada. Progresso: " + ProgressText();
        }

        private void ValidateMission()
        {
            if (!gameplay.CharacterIsControlled)
            {
                return;
            }
            var success = mission.Evaluate(loadout) == MissionResult.Success;
            gameplay.RegisterValidation(success);
            feedback = success
                ? "Muito bem! 2 + 3 = 5. Vá ao embarque e pressione Espaço."
                : "Ainda não. Confira as quantidades: " + ProgressText();
        }

        private void ResetMission()
        {
            if (character != null && world != null)
            {
                character.ResetToSpawn(world);
            }
            if (raft != null)
            {
                raft.ResetPose();
            }
            loadout.Clear();
            for (var i = 0; i < placedVisuals.Length; i++)
            {
                DestroyPlacedVisual(i);
            }
            selectedCargo = null;
            gameplay.Reset();
            RefreshCarriedVisual();
            feedback = "Missão iniciada: use as setas e carregue 2 peixes e 3 cocos.";
        }

        private bool IsRaftInDeliveryZone()
        {
            return deliveryZone != null && raft != null && deliveryZone.Contains(raft.transform.position);
        }

        private void RefreshCarriedVisual()
        {
            if (carriedVisual != null)
            {
                DestroyRuntimeObject(carriedVisual);
                carriedVisual = null;
            }
            if (selectedCargo.HasValue && character != null && character.CarryAnchor != null)
            {
                carriedVisual = BlockoutFactory.CreateCargoObject(selectedCargo.Value, "Carga_na_Mao", character.CarryAnchor, false);
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            var panel = GetInterfaceRect();
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label("Jangada dos Números", titleStyle);
            GUILayout.Label("Missão: 2 peixes + 3 cocos = 5  •  " + ProgressText(), bodyStyle);
            GUILayout.Label(ControlText() + "  •  " + (selectedCargo.HasValue ? "Na mão: " + DisplayName(selectedCargo.Value) : "Mãos livres"), bodyStyle);
            GUILayout.Label(feedback, feedbackStyle);
            GUILayout.EndVertical();
            GUILayout.BeginVertical(GUILayout.Width(GetButtonColumnWidth(panel.width)));
            GUI.enabled = gameplay.CharacterIsControlled;
            if (GUILayout.Button("Validar", GUILayout.Height(42f)))
            {
                ValidateMission();
            }
            GUI.enabled = true;
            if (GUILayout.Button(gameplay.Phase == GameplayPhase.Delivered ? "Nova missão" : "Reiniciar", GUILayout.Height(42f)))
            {
                ResetMission();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private string ControlText()
        {
            switch (gameplay.Phase)
            {
                case GameplayPhase.Sailing: return IsRaftInDeliveryZone() ? "Espaço: entregar" : "Setas: navegar";
                case GameplayPhase.Delivered: return "Missão completa";
                case GameplayPhase.ReadyToSail: return "Setas: mover  •  Espaço: embarcar";
                default: return "Setas: mover  •  Espaço: pegar/colocar";
            }
        }

        private string ProgressText()
        {
            return loadout.Count(CargoType.Peixe) + "/2 peixes, " + loadout.Count(CargoType.Coco) +
                   "/3 cocos, " + loadout.OccupiedCount + "/5 total";
        }

        private static string DisplayName(CargoType cargo)
        {
            switch (cargo)
            {
                case CargoType.Peixe: return "peixe";
                case CargoType.Coco: return "coco";
                case CargoType.Cesto: return "cesto";
                case CargoType.Tecido: return "tecido";
                case CargoType.Ceramica: return "cerâmica";
                default: return "fruta";
            }
        }

        private void DestroyPlacedVisual(int index)
        {
            if (placedVisuals[index] == null)
            {
                return;
            }
            DestroyRuntimeObject(placedVisuals[index]);
            placedVisuals[index] = null;
        }

        private static void DestroyRuntimeObject(GameObject item)
        {
            if (Application.isPlaying) Object.Destroy(item);
            else Object.DestroyImmediate(item);
        }

        private static bool TryGetPointerRelease(out Vector2 position)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                position = touch.position;
                return touch.phase == TouchPhase.Ended;
            }
            position = Input.mousePosition;
            return Input.GetMouseButtonUp(0);
        }

        private static bool IsOverInterface(Vector2 pointerPosition)
        {
            var guiPointer = new Vector2(pointerPosition.x, Screen.height - pointerPosition.y);
            return GetInterfaceRect().Contains(guiPointer);
        }

        public static Rect CalculateInterfaceRect(Rect safeArea, float screenHeight)
        {
            const float margin = 10f;
            var width = Mathf.Max(0f, safeArea.width - margin * 2f);
            var preferredHeight = safeArea.width < 700f ? 176f : 146f;
            var height = Mathf.Min(preferredHeight, Mathf.Max(0f, safeArea.height - margin * 2f));
            var x = safeArea.x + margin;
            var safeTopInGui = screenHeight - safeArea.yMax;
            var y = safeTopInGui + safeArea.height - height - margin;
            return new Rect(x, y, width, height);
        }

        private static Rect GetInterfaceRect()
        {
            return CalculateInterfaceRect(Screen.safeArea, Screen.height);
        }

        private static float GetButtonColumnWidth(float panelWidth)
        {
            return Mathf.Clamp(panelWidth * 0.22f, 104f, 190f);
        }

        private void EnsureStyles()
        {
            var scale = Mathf.Clamp(Screen.dpi > 0f ? Screen.dpi / 160f : 1f, 1f, 1.35f);
            var styleSize = Mathf.RoundToInt(15f * scale);
            if (titleStyle != null && cachedStyleSize == styleSize) return;
            cachedStyleSize = styleSize;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(20f * scale), fontStyle = FontStyle.Bold, wordWrap = false };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = styleSize, wordWrap = true };
            feedbackStyle = new GUIStyle(bodyStyle) { fontSize = Mathf.Max(13, styleSize - 1), wordWrap = true };
        }
    }
}
