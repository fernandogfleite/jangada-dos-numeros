using JangadaDosNumeros.Core;
using NUnit.Framework;

namespace JangadaDosNumeros.Tests
{
    public sealed class GameplaySessionTests
    {
        [Test]
        public void ValidationAndBoarding_RequireCorrectStateAndProximity()
        {
            var session = new GameplaySession();

            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.Loading));
            Assert.That(session.TryBoard(true), Is.False);

            session.RegisterValidation(false);
            Assert.That(session.TryBoard(true), Is.False);

            session.RegisterValidation(true);
            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.ReadyToSail));
            Assert.That(session.TryBoard(false), Is.False);
            Assert.That(session.TryBoard(true), Is.True);
            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.Sailing));
            Assert.That(session.RaftIsControlled, Is.True);
            Assert.That(session.CharacterIsControlled, Is.False);
        }

        [Test]
        public void Delivery_OnlyCompletesWhileSailingInsideZone()
        {
            var session = new GameplaySession();

            Assert.That(session.TryDeliver(true), Is.False);
            session.RegisterValidation(true);
            session.TryBoard(true);
            Assert.That(session.TryDeliver(false), Is.False);
            Assert.That(session.TryDeliver(true), Is.True);
            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.Delivered));
            Assert.That(session.CanMove, Is.False);
        }

        [Test]
        public void LoadChangeRevokesReadinessAndResetRestoresInitialState()
        {
            var session = new GameplaySession();
            session.RegisterValidation(true);

            session.RevokeValidation();
            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.Loading));

            session.RegisterValidation(true);
            session.TryBoard(true);
            session.TryDeliver(true);
            session.Reset();

            Assert.That(session.Phase, Is.EqualTo(GameplayPhase.Loading));
            Assert.That(session.CharacterIsControlled, Is.True);
            Assert.That(session.CanMove, Is.True);
        }
    }
}
