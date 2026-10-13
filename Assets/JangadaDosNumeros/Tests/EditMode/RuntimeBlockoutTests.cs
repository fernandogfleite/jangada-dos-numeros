using System.Linq;
using JangadaDosNumeros.Core;
using JangadaDosNumeros.Runtime;
using NUnit.Framework;
using UnityEngine;

namespace JangadaDosNumeros.Tests
{
    public sealed class RuntimeBlockoutTests
    {
        private GameObject owner;

        [TearDown]
        public void TearDown()
        {
            if (owner != null)
            {
                Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void BuildWorld_CreatesEightNamedSlotsWithNeutralAnchors()
        {
            owner = new GameObject("TestWorld");
            BlockoutFactory.BuildWorld(owner.transform);

            var slots = owner.GetComponentsInChildren<SlotView>();

            Assert.That(slots.Length, Is.EqualTo(8));
            Assert.That(slots.Select(slot => slot.name), Is.EquivalentTo(
                Enumerable.Range(1, 8).Select(index => "Slot_" + index.ToString("00"))));
            foreach (var slot in slots)
            {
                Assert.That(slot.transform.lossyScale, Is.EqualTo(Vector3.one));
                Assert.That(slot.CargoAnchor.lossyScale, Is.EqualTo(Vector3.one));
            }
        }

        [TestCase(CargoType.Peixe)]
        [TestCase(CargoType.Coco)]
        [TestCase(CargoType.Cesto)]
        [TestCase(CargoType.Tecido)]
        [TestCase(CargoType.Ceramica)]
        [TestCase(CargoType.Fruta)]
        public void CargoPlacedAtAnchor_HasCompositeSilhouetteThatFitsSlotAndSitsOnBase(CargoType cargoType)
        {
            owner = new GameObject("TestWorld");
            BlockoutFactory.BuildWorld(owner.transform);
            var slot = owner.GetComponentsInChildren<SlotView>()[0];
            var cargo = BlockoutFactory.CreateCargoObject(cargoType, "TestCargo", slot.CargoAnchor, false);
            var markerRenderer = slot.transform.Find("Marcador").GetComponent<Renderer>();
            var cargoBounds = CombinedBounds(cargo.GetComponentsInChildren<Renderer>());

            Assert.That(cargo.transform.lossyScale, Is.EqualTo(Vector3.one));
            Assert.That(cargo.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(1));
            Assert.That(cargoBounds.size.x, Is.LessThanOrEqualTo(0.62f));
            Assert.That(cargoBounds.size.y, Is.LessThanOrEqualTo(0.62f));
            Assert.That(cargoBounds.size.z, Is.LessThanOrEqualTo(0.5f));
            Assert.That(cargoBounds.min.y, Is.EqualTo(slot.CargoAnchor.position.y).Within(0.025f));
            Assert.That(cargoBounds.min.y, Is.GreaterThanOrEqualTo(markerRenderer.bounds.max.y - 0.025f));
        }

        [Test]
        public void AdaptiveCamera_UsesPerspectiveAndMovesBackForFourByThree()
        {
            var cameraObject = new GameObject("TestCamera");
            var camera = cameraObject.AddComponent<Camera>();
            var adaptive = cameraObject.AddComponent<AdaptiveCameraView>();

            var widescreenDistance = adaptive.CalculateCameraDistance(16f / 9f);
            var fourByThreeDistance = adaptive.CalculateCameraDistance(4f / 3f);
            adaptive.ApplyFramingForAspect(4f / 3f);

            Assert.That(camera.orthographic, Is.False);
            Assert.That(camera.fieldOfView, Is.EqualTo(42f).Within(0.01f));
            Assert.That(fourByThreeDistance, Is.GreaterThan(widescreenDistance));
            Object.DestroyImmediate(cameraObject);
        }

        [TestCase(16f / 9f)]
        [TestCase(4f / 3f)]
        public void AdaptiveCamera_KeepsAtelierRaftAndFairInsideViewport(float aspect)
        {
            owner = new GameObject("TestWorld");
            BlockoutFactory.BuildWorld(owner.transform);
            var camera = owner.GetComponentInChildren<Camera>();
            camera.aspect = aspect;
            camera.GetComponent<AdaptiveCameraView>().ApplyFramingForAspect(aspect);

            AssertInsideViewport(camera, owner.transform.Find("World_Blockout/Atelie_de_Cargas").position);
            AssertInsideViewport(camera, owner.transform.Find("World_Blockout/Jangada_3_6m").position);
            AssertInsideViewport(camera, owner.transform.Find("World_Blockout/Feira_Destino").position);
        }

        [Test]
        public void InterfaceLayout_StaysInBottomSafeAreaAndAdaptsToNarrowScreens()
        {
            var wide = VerticalSliceController.CalculateInterfaceRect(new Rect(0f, 0f, 1920f, 1080f), 1080f);
            var narrow = VerticalSliceController.CalculateInterfaceRect(new Rect(0f, 0f, 640f, 480f), 480f);

            Assert.That(wide.yMin, Is.GreaterThan(900f));
            Assert.That(wide.yMax, Is.LessThanOrEqualTo(1080f));
            Assert.That(narrow.height, Is.GreaterThan(wide.height));
            Assert.That(narrow.yMax, Is.LessThanOrEqualTo(480f));
        }

        [Test]
        public void BuildWorld_CreatesRecognizableThreeDimensionalLandmarks()
        {
            owner = new GameObject("TestWorld");
            BlockoutFactory.BuildWorld(owner.transform);
            var world = owner.transform.Find("World_Blockout");

            Assert.That(world.Find("Jangada_3_6m/Mastro"), Is.Not.Null);
            Assert.That(world.Find("Jangada_3_6m/Vela"), Is.Not.Null);
            Assert.That(world.Find("Atelie_de_Cargas/Telhado_Esq"), Is.Not.Null);
            Assert.That(world.Find("Feira_Destino/Toldo_01"), Is.Not.Null);
            Assert.That(world.Find("Cenario_Lagoa/Vegetacao_Oeste_01"), Is.Not.Null);
        }

        [Test]
        public void BuildWorld_CreatesPlayableCharacterMobileRaftAndDeliveryZone()
        {
            owner = new GameObject("TestWorld");
            BlockoutFactory.BuildWorld(owner.transform);
            var character = owner.GetComponentInChildren<PlayerCharacterController>();
            var raft = owner.GetComponentInChildren<RaftNavigationView>();
            var delivery = owner.GetComponentInChildren<DeliveryZoneView>();
            var slots = owner.GetComponentsInChildren<SlotView>();

            Assert.That(character, Is.Not.Null);
            Assert.That(character.CarryAnchor, Is.Not.Null);
            Assert.That(raft, Is.Not.Null);
            Assert.That(raft.LoadPoint, Is.Not.Null);
            Assert.That(raft.BoardingPoint, Is.Not.Null);
            Assert.That(raft.DriverAnchor, Is.Not.Null);
            Assert.That(delivery, Is.Not.Null);
            Assert.That(slots.All(slot => slot.transform.IsChildOf(raft.transform)), Is.True);
        }

        [Test]
        public void MovementClamps_KeepActorsInsideTheirPlayableAreas()
        {
            var characterPoint = PlayerCharacterController.ClampToWalkableArea(new Vector3(20f, 0.22f, -20f));
            var raftPoint = RaftNavigationView.ClampToNavigableArea(new Vector3(20f, 0.18f, -20f));

            Assert.That(characterPoint.x, Is.InRange(-7.35f, -0.6f));
            Assert.That(characterPoint.z, Is.InRange(-2.25f, 3.8f));
            Assert.That(raftPoint.x, Is.InRange(-0.15f, 4.25f));
            Assert.That(raftPoint.z, Is.InRange(-3.6f, 1.05f));
        }

        [Test]
        public void InterfaceLayout_DoesNotOverflowSmallSafeArea()
        {
            var safeArea = new Rect(12f, 20f, 240f, 170f);
            var panel = VerticalSliceController.CalculateInterfaceRect(safeArea, 220f);
            var safeAreaInGui = new Rect(safeArea.x, 220f - safeArea.yMax, safeArea.width, safeArea.height);

            Assert.That(panel.xMin, Is.GreaterThanOrEqualTo(safeAreaInGui.xMin));
            Assert.That(panel.xMax, Is.LessThanOrEqualTo(safeAreaInGui.xMax));
            Assert.That(panel.yMin, Is.GreaterThanOrEqualTo(safeAreaInGui.yMin));
            Assert.That(panel.yMax, Is.LessThanOrEqualTo(safeAreaInGui.yMax));
        }

        private static Bounds CombinedBounds(Renderer[] renderers)
        {
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void AssertInsideViewport(Camera camera, Vector3 worldPosition)
        {
            var viewport = camera.WorldToViewportPoint(worldPosition);
            Assert.That(viewport.z, Is.GreaterThan(0f));
            Assert.That(viewport.x, Is.InRange(0.04f, 0.96f));
            Assert.That(viewport.y, Is.InRange(0.16f, 0.96f));
        }
    }
}
