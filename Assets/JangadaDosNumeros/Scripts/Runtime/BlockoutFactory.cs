using System;
using JangadaDosNumeros.Core;
using UnityEngine;

namespace JangadaDosNumeros.Runtime
{
    public static class BlockoutFactory
    {
        private static readonly Color Water = new Color(0.08f, 0.48f, 0.68f);
        private static readonly Color Wood = new Color(0.42f, 0.23f, 0.08f);
        private static readonly Color Sand = new Color(0.78f, 0.68f, 0.42f);
        private static readonly Color Rope = new Color(0.74f, 0.58f, 0.3f);
        private static readonly Color Leaf = new Color(0.2f, 0.55f, 0.22f);

        public static void BuildWorld(Transform owner)
        {
            var existing = owner.Find("World_Blockout");
            if (existing != null)
            {
                existing.name = "World_Blockout_Antigo";
                existing.gameObject.SetActive(false);
                DestroyObject(existing.gameObject);
            }

            var world = NewRoot("World_Blockout", owner);
            BuildLagoon(world);
            BuildRaft(world);
            BuildAtelier(world);
            BuildFair(world);
            BuildCharacter(world);
            BuildCameraAndLight(world);
        }

        public static GameObject CreateCargoObject(CargoType cargo, string objectName, Transform parent, bool interactive)
        {
            var root = new GameObject(objectName);
            root.transform.SetParent(parent, false);
            BuildCargoVisual(cargo, root.transform);

            if (interactive)
            {
                var view = root.AddComponent<CargoPrototypeView>();
                view.CargoType = cargo;
            }

            return root;
        }

        private static void BuildLagoon(Transform parent)
        {
            var environment = NewRoot("Cenario_Lagoa", parent);
            CreatePrimitive("Agua", PrimitiveType.Plane, environment, new Vector3(0f, -0.08f, 0f), new Vector3(1.8f, 1f, 1.55f), Water);
            CreatePrimitive("Margem_Norte", PrimitiveType.Cube, environment, new Vector3(0f, -0.2f, 7.5f), new Vector3(18f, 0.42f, 2.4f), Sand);
            CreatePrimitive("Ilha_Oeste", PrimitiveType.Cylinder, environment, new Vector3(-7.2f, -0.12f, 0.7f), new Vector3(2.15f, 0.18f, 1.9f), Sand);
            CreatePrimitive("Ilha_Leste", PrimitiveType.Cylinder, environment, new Vector3(7.2f, -0.12f, 1.3f), new Vector3(2.1f, 0.18f, 1.75f), Sand);
            for (var i = 0; i < 6; i++)
            {
                CreatePrimitive("Reflexo_" + (i + 1).ToString("00"), PrimitiveType.Cube, environment,
                    new Vector3(-6.5f + i * 2.55f, -0.02f, -3.8f + (i % 2) * 0.7f),
                    new Vector3(1.2f, 0.015f, 0.05f), new Color(0.36f, 0.78f, 0.86f));
            }

            var dock = NewRoot("Cais", environment);
            for (var plank = 0; plank < 7; plank++)
            {
                CreatePrimitive("Tabua_" + (plank + 1).ToString("00"), PrimitiveType.Cube, dock,
                    new Vector3(-4.7f + plank * 0.34f, 0.26f + (plank % 2) * 0.025f, -0.35f),
                    new Vector3(0.31f, 0.22f, 4.1f), plank % 2 == 0 ? Wood : new Color(0.5f, 0.3f, 0.12f));
            }
            for (var i = 0; i < 4; i++)
            {
                CreatePrimitive("Pilar_" + (i + 1).ToString("00"), PrimitiveType.Cylinder, dock,
                    new Vector3(-4.65f + (i % 2) * 1.7f, -0.2f, -1.9f + (i / 2) * 3.1f),
                    new Vector3(0.18f, 0.7f, 0.18f), Wood);
            }

            CreatePrimitive("Passarela_da_Jangada", PrimitiveType.Cube, dock,
                new Vector3(-2.15f, 0.3f, 0f), new Vector3(1.05f, 0.13f, 0.9f), Wood);

            BuildVegetation(environment, new Vector3(-7.5f, 0.05f, 1.7f), "Oeste");
            BuildVegetation(environment, new Vector3(7.2f, 0.05f, 2.2f), "Leste");
        }

        private static void BuildRaft(Transform parent)
        {
            var raft = NewRoot("Jangada_3_6m", parent);
            raft.localPosition = new Vector3(0f, 0.18f, 0f);
            raft.localRotation = Quaternion.Euler(0f, 90f, 0f);
            for (var i = 0; i < 4; i++)
            {
                CreatePrimitive("Tronco_" + (i + 1).ToString("00"), PrimitiveType.Cylinder, raft,
                    new Vector3(-0.78f + i * 0.52f, 0f, 0f), new Vector3(0.24f, 1.8f, 0.24f),
                    new Vector3(90f, 0f, 0f), i % 2 == 0 ? Wood : new Color(0.5f, 0.29f, 0.1f));
            }

            for (var tie = 0; tie < 3; tie++)
            {
                CreatePrimitive("Amarracao_" + (tie + 1).ToString("00"), PrimitiveType.Cylinder, raft,
                    new Vector3(0f, 0.24f, -1.35f + tie * 1.35f), new Vector3(0.055f, 1.15f, 0.055f),
                    new Vector3(0f, 0f, 90f), Rope);
            }

            CreatePrimitive("Mastro", PrimitiveType.Cylinder, raft, new Vector3(1.05f, 1.25f, 0.8f), new Vector3(0.07f, 1.2f, 0.07f), Wood);
            CreatePrimitive("Vela", PrimitiveType.Cube, raft, new Vector3(1.05f, 1.65f, 0.35f), new Vector3(0.04f, 1.25f, 0.82f), new Vector3(0f, 0f, -8f), new Color(0.96f, 0.84f, 0.48f));
            CreatePrimitive("Remo", PrimitiveType.Cylinder, raft, new Vector3(-1.15f, 0.34f, 0f), new Vector3(0.045f, 1.45f, 0.045f), new Vector3(76f, 0f, 8f), Rope);
            CreatePrimitive("Pa_do_Remo", PrimitiveType.Cube, raft, new Vector3(-1.15f, 0.15f, 1.43f), new Vector3(0.22f, 0.06f, 0.48f), Rope);

            var slots = NewRoot("Slots_2x4", raft);
            var index = 0;
            for (var row = 0; row < 4; row++)
            {
                for (var column = 0; column < 2; column++)
                {
                    var slot = new GameObject("Slot_" + (index + 1).ToString("00"));
                    slot.transform.SetParent(slots, false);
                    slot.transform.localPosition = new Vector3(-0.46f + column * 0.92f, 0.18f, -1.2f + row * 0.8f);
                    CreatePrimitive("Marcador", PrimitiveType.Cube, slot.transform,
                        new Vector3(0f, 0.04f, 0f), new Vector3(0.54f, 0.08f, 0.54f),
                        new Color(0.96f, 0.8f, 0.16f));
                    var anchor = new GameObject("CargoAnchor").transform;
                    anchor.SetParent(slot.transform, false);
                    anchor.localPosition = new Vector3(0f, 0.08f, 0f);
                    anchor.localScale = Vector3.one;
                    var view = slot.AddComponent<SlotView>();
                    view.Index = index;
                    view.CargoAnchor = anchor;
                    index++;
                }
            }

            var loadPoint = NewRoot("Ponto_de_Carga", raft);
            loadPoint.localPosition = new Vector3(0f, 0.34f, -1.72f);
            var boardingPoint = NewRoot("Ponto_de_Embarque", raft);
            boardingPoint.localPosition = new Vector3(-0.72f, 0.34f, -1.45f);
            var driverAnchor = NewRoot("Assento_do_Personagem", raft);
            driverAnchor.localPosition = new Vector3(-0.78f, 0.46f, 0.76f);
            var raftView = raft.gameObject.AddComponent<RaftNavigationView>();
            raftView.LoadPoint = loadPoint;
            raftView.BoardingPoint = boardingPoint;
            raftView.DriverAnchor = driverAnchor;
            raftView.CaptureInitialPose();
        }

        private static void BuildAtelier(Transform parent)
        {
            var atelier = NewRoot("Atelie_de_Cargas", parent);
            atelier.localPosition = new Vector3(-5.9f, 0f, 2.7f);
            CreatePrimitive("Piso", PrimitiveType.Cube, atelier, Vector3.zero, new Vector3(3.5f, 0.22f, 3.1f), new Color(0.58f, 0.36f, 0.17f));
            CreatePrimitive("Parede_Fundos", PrimitiveType.Cube, atelier, new Vector3(0f, 1.25f, 1.32f), new Vector3(3.5f, 2.5f, 0.18f), new Color(0.92f, 0.66f, 0.32f));
            CreatePrimitive("Parede_Lateral", PrimitiveType.Cube, atelier, new Vector3(-1.66f, 1.25f, 0.35f), new Vector3(0.18f, 2.5f, 2.1f), new Color(0.84f, 0.52f, 0.25f));
            CreatePrimitive("Telhado_Esq", PrimitiveType.Cube, atelier, new Vector3(-0.83f, 2.65f, 0.45f), new Vector3(1.95f, 0.16f, 3.45f), new Vector3(0f, 0f, 18f), new Color(0.62f, 0.16f, 0.09f));
            CreatePrimitive("Telhado_Dir", PrimitiveType.Cube, atelier, new Vector3(0.83f, 2.65f, 0.45f), new Vector3(1.95f, 0.16f, 3.45f), new Vector3(0f, 0f, -18f), new Color(0.72f, 0.22f, 0.1f));
            CreatePrimitive("Varanda", PrimitiveType.Cube, atelier, new Vector3(0f, 0.12f, -1.8f), new Vector3(3.6f, 0.18f, 0.7f), Wood);
            CreatePrimitive("Prateleira", PrimitiveType.Cube, atelier, new Vector3(0f, 0.7f, 1.12f), new Vector3(2.7f, 0.12f, 0.42f), Wood);

            foreach (CargoType cargo in Enum.GetValues(typeof(CargoType)))
            {
                var value = (int)cargo;
                var item = CreateCargoObject(cargo, "Carga_" + cargo, atelier, true);
                item.transform.localPosition = new Vector3(-1f + (value % 3), 0.18f, -0.78f + (value / 3) * 0.92f);
            }
        }

        private static void BuildFair(Transform parent)
        {
            var fair = NewRoot("Feira_Destino", parent);
            fair.gameObject.AddComponent<DestinationView>();
            fair.localPosition = new Vector3(5.6f, 0f, 2.9f);
            CreatePrimitive("Balcao", PrimitiveType.Cube, fair, new Vector3(0f, 0.5f, 0f), new Vector3(3.2f, 1f, 1.1f), new Color(0.72f, 0.22f, 0.12f));
            for (var stripe = 0; stripe < 6; stripe++)
            {
                CreatePrimitive("Toldo_" + (stripe + 1).ToString("00"), PrimitiveType.Cube, fair,
                    new Vector3(-1.55f + stripe * 0.62f, 2.12f, 0f), new Vector3(0.64f, 0.16f, 2.25f),
                    stripe % 2 == 0 ? new Color(0.96f, 0.75f, 0.12f) : new Color(0.92f, 0.28f, 0.12f));
            }
            for (var i = 0; i < 2; i++)
            {
                CreatePrimitive("Poste_" + (i + 1).ToString("00"), PrimitiveType.Cube, fair,
                    new Vector3(-1.45f + i * 2.9f, 1.1f, 0f), new Vector3(0.16f, 2.2f, 0.16f), Wood);
            }
            CreatePrimitive("Caixote_01", PrimitiveType.Cube, fair, new Vector3(-1.05f, 1.1f, -0.05f), new Vector3(0.55f, 0.25f, 0.55f), Rope);
            CreatePrimitive("Caixote_02", PrimitiveType.Cube, fair, new Vector3(0.95f, 1.1f, -0.05f), new Vector3(0.55f, 0.25f, 0.55f), Rope);

            var deliveryZone = NewRoot("Zona_de_Entrega", fair);
            deliveryZone.localPosition = new Vector3(-1.6f, 0.04f, -2.9f);
            var zone = deliveryZone.gameObject.AddComponent<DeliveryZoneView>();
            zone.Radius = 1.15f;
            var marker = CreatePrimitive("Marcador_de_Atracacao", PrimitiveType.Cylinder, deliveryZone,
                Vector3.zero, new Vector3(1.15f, 0.025f, 1.15f), new Color(0.25f, 0.85f, 0.48f));
            var collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }
        }

        private static void BuildCharacter(Transform parent)
        {
            var character = NewRoot("Personagem_Jogavel", parent);
            character.localPosition = new Vector3(-5.9f, 0.22f, 0.82f);

            CreatePrimitive("Corpo", PrimitiveType.Capsule, character, new Vector3(0f, 0.62f, 0f),
                new Vector3(0.38f, 0.56f, 0.32f), new Color(0.12f, 0.42f, 0.72f));
            CreatePrimitive("Cabeca", PrimitiveType.Sphere, character, new Vector3(0f, 1.35f, 0f),
                new Vector3(0.42f, 0.42f, 0.42f), new Color(0.62f, 0.35f, 0.19f));
            CreatePrimitive("Chapeu", PrimitiveType.Cylinder, character, new Vector3(0f, 1.62f, 0f),
                new Vector3(0.38f, 0.08f, 0.38f), new Color(0.91f, 0.67f, 0.2f));
            CreatePrimitive("Braco_Esquerdo", PrimitiveType.Capsule, character, new Vector3(-0.3f, 0.72f, 0f),
                new Vector3(0.13f, 0.34f, 0.13f), new Vector3(0f, 0f, -12f), new Color(0.62f, 0.35f, 0.19f));
            CreatePrimitive("Braco_Direito", PrimitiveType.Capsule, character, new Vector3(0.3f, 0.72f, 0f),
                new Vector3(0.13f, 0.34f, 0.13f), new Vector3(0f, 0f, 12f), new Color(0.62f, 0.35f, 0.19f));
            CreatePrimitive("Perna_Esquerda", PrimitiveType.Capsule, character, new Vector3(-0.14f, 0.18f, 0f),
                new Vector3(0.14f, 0.3f, 0.14f), new Color(0.2f, 0.18f, 0.12f));
            CreatePrimitive("Perna_Direita", PrimitiveType.Capsule, character, new Vector3(0.14f, 0.18f, 0f),
                new Vector3(0.14f, 0.3f, 0.14f), new Color(0.2f, 0.18f, 0.12f));

            var carryAnchor = NewRoot("Ponto_da_Mao", character);
            carryAnchor.localPosition = new Vector3(0f, 1.02f, 0.38f);
            var controller = character.gameObject.AddComponent<PlayerCharacterController>();
            controller.CarryAnchor = carryAnchor;
            controller.CaptureSpawnPose();
        }

        private static void BuildCameraAndLight(Transform parent)
        {
            var cameraObject = new GameObject("Camera_Principal");
            cameraObject.transform.SetParent(parent, false);
            var camera = cameraObject.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = false;
            camera.fieldOfView = 42f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.56f, 0.79f, 0.9f);
            var adaptive = cameraObject.AddComponent<AdaptiveCameraView>();
            adaptive.ApplyFramingForAspect(16f / 9f);

            var lightObject = new GameObject("Luz_Direcional");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.91f, 0.74f);
            light.shadows = LightShadows.Soft;

            var fillObject = new GameObject("Luz_Preenchimento");
            fillObject.transform.SetParent(parent, false);
            fillObject.transform.position = new Vector3(0f, 5f, -2f);
            var fill = fillObject.AddComponent<Light>();
            fill.type = LightType.Point;
            fill.range = 18f;
            fill.intensity = 0.55f;
            fill.color = new Color(0.42f, 0.68f, 1f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.5f, 0.68f, 0.76f);
            RenderSettings.ambientEquatorColor = new Color(0.34f, 0.48f, 0.43f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.2f, 0.17f);
        }

        private static void BuildCargoVisual(CargoType cargo, Transform root)
        {
            switch (cargo)
            {
                case CargoType.Peixe:
                    CreatePrimitive("Visual_Corpo", PrimitiveType.Sphere, root, new Vector3(0.02f, 0.13f, 0f), new Vector3(0.38f, 0.2f, 0.2f), new Color(0.15f, 0.65f, 0.9f));
                    CreatePrimitive("Cauda", PrimitiveType.Cube, root, new Vector3(-0.23f, 0.13f, 0f), new Vector3(0.16f, 0.17f, 0.05f), new Vector3(0f, 0f, 45f), new Color(0.1f, 0.52f, 0.78f));
                    CreatePrimitive("Olho", PrimitiveType.Sphere, root, new Vector3(0.17f, 0.17f, -0.085f), new Vector3(0.035f, 0.035f, 0.025f), new Color(0.04f, 0.04f, 0.04f));
                    break;
                case CargoType.Coco:
                    CreatePrimitive("Visual_Coco", PrimitiveType.Sphere, root, new Vector3(0f, 0.16f, 0f), new Vector3(0.32f, 0.32f, 0.3f), new Color(0.29f, 0.5f, 0.17f));
                    for (var mark = -1; mark <= 1; mark++)
                    {
                        CreatePrimitive("Marca_" + (mark + 2), PrimitiveType.Sphere, root, new Vector3(mark * 0.055f, 0.285f, -0.09f), new Vector3(0.028f, 0.025f, 0.018f), new Color(0.18f, 0.3f, 0.09f));
                    }
                    break;
                case CargoType.Cesto:
                    CreatePrimitive("Visual_Cesto", PrimitiveType.Cylinder, root, new Vector3(0f, 0.14f, 0f), new Vector3(0.3f, 0.14f, 0.3f), new Color(0.72f, 0.43f, 0.12f));
                    CreatePrimitive("Alca_Esq", PrimitiveType.Cube, root, new Vector3(-0.13f, 0.31f, 0f), new Vector3(0.035f, 0.22f, 0.035f), new Vector3(0f, 0f, -20f), Rope);
                    CreatePrimitive("Alca_Dir", PrimitiveType.Cube, root, new Vector3(0.13f, 0.31f, 0f), new Vector3(0.035f, 0.22f, 0.035f), new Vector3(0f, 0f, 20f), Rope);
                    CreatePrimitive("Alca_Topo", PrimitiveType.Cube, root, new Vector3(0f, 0.415f, 0f), new Vector3(0.23f, 0.035f, 0.035f), Rope);
                    break;
                case CargoType.Tecido:
                    CreatePrimitive("Visual_Dobra_01", PrimitiveType.Cube, root, new Vector3(0f, 0.055f, 0f), new Vector3(0.42f, 0.11f, 0.34f), new Color(0.7f, 0.18f, 0.65f));
                    CreatePrimitive("Dobra_02", PrimitiveType.Cube, root, new Vector3(0.03f, 0.13f, 0.01f), new Vector3(0.35f, 0.06f, 0.3f), new Color(0.88f, 0.32f, 0.72f));
                    CreatePrimitive("Faixa", PrimitiveType.Cube, root, new Vector3(0f, 0.165f, 0.01f), new Vector3(0.07f, 0.02f, 0.31f), new Color(0.98f, 0.72f, 0.18f));
                    break;
                case CargoType.Ceramica:
                    CreatePrimitive("Visual_Vaso", PrimitiveType.Sphere, root, new Vector3(0f, 0.2f, 0f), new Vector3(0.31f, 0.4f, 0.31f), new Color(0.85f, 0.35f, 0.12f));
                    CreatePrimitive("Gargalo", PrimitiveType.Cylinder, root, new Vector3(0f, 0.42f, 0f), new Vector3(0.12f, 0.09f, 0.12f), new Color(0.76f, 0.25f, 0.08f));
                    CreatePrimitive("Borda", PrimitiveType.Cylinder, root, new Vector3(0f, 0.52f, 0f), new Vector3(0.16f, 0.025f, 0.16f), new Color(0.94f, 0.48f, 0.17f));
                    break;
                default:
                    CreatePrimitive("Visual_Fruta_01", PrimitiveType.Sphere, root, new Vector3(-0.09f, 0.13f, 0f), new Vector3(0.23f, 0.25f, 0.23f), new Color(0.88f, 0.12f, 0.2f));
                    CreatePrimitive("Fruta_02", PrimitiveType.Sphere, root, new Vector3(0.09f, 0.13f, 0.02f), new Vector3(0.23f, 0.25f, 0.23f), new Color(0.96f, 0.38f, 0.08f));
                    CreatePrimitive("Folha", PrimitiveType.Cube, root, new Vector3(0.01f, 0.29f, 0f), new Vector3(0.18f, 0.025f, 0.08f), new Vector3(0f, 25f, 18f), Leaf);
                    break;
            }
        }

        private static void BuildVegetation(Transform parent, Vector3 origin, string suffix)
        {
            for (var i = 0; i < 3; i++)
            {
                var plant = NewRoot("Vegetacao_" + suffix + "_" + (i + 1).ToString("00"), parent);
                plant.localPosition = origin + new Vector3(i * 0.7f, 0f, (i % 2) * 0.55f);
                CreatePrimitive("Caule", PrimitiveType.Cylinder, plant, new Vector3(0f, 0.45f, 0f), new Vector3(0.055f, 0.45f, 0.055f), Wood);
                for (var leaf = 0; leaf < 3; leaf++)
                {
                    CreatePrimitive("Folha_" + (leaf + 1), PrimitiveType.Sphere, plant,
                        new Vector3((leaf - 1) * 0.14f, 0.9f + (leaf % 2) * 0.08f, 0f),
                        new Vector3(0.28f, 0.16f, 0.18f), Leaf);
                }
            }
        }

        private static Transform NewRoot(string name, Transform parent)
        {
            var root = new GameObject(name).transform;
            root.SetParent(parent, false);
            return root;
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType primitive, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            return CreatePrimitive(name, primitive, parent, localPosition, scale, Vector3.zero, color);
        }

        private static GameObject CreatePrimitive(string name, PrimitiveType primitive, Transform parent, Vector3 localPosition, Vector3 scale, Vector3 localEulerAngles, Color color)
        {
            var item = GameObject.CreatePrimitive(primitive);
            item.name = name;
            item.transform.SetParent(parent, false);
            item.transform.localPosition = localPosition;
            item.transform.localEulerAngles = localEulerAngles;
            item.transform.localScale = scale;
            ApplyColor(item, color);
            return item;
        }

        private static void ApplyColor(GameObject item, Color color)
        {
            var renderer = item.GetComponent<Renderer>();
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_BaseColor", color);
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
        }

        private static void DestroyObject(GameObject item)
        {
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(item);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(item);
            }
        }
    }
}

