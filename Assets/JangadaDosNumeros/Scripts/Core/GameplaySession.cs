namespace JangadaDosNumeros.Core
{
    public enum GameplayPhase
    {
        Loading,
        ReadyToSail,
        Sailing,
        Delivered
    }

    public sealed class GameplaySession
    {
        public GameplayPhase Phase { get; private set; }

        public bool CharacterIsControlled => Phase == GameplayPhase.Loading || Phase == GameplayPhase.ReadyToSail;

        public bool RaftIsControlled => Phase == GameplayPhase.Sailing;

        public bool CanMove => Phase != GameplayPhase.Delivered;

        public GameplaySession()
        {
            Reset();
        }

        public void RegisterValidation(bool isCorrect)
        {
            if (Phase == GameplayPhase.Loading || Phase == GameplayPhase.ReadyToSail)
            {
                Phase = isCorrect ? GameplayPhase.ReadyToSail : GameplayPhase.Loading;
            }
        }

        public void RevokeValidation()
        {
            if (Phase == GameplayPhase.ReadyToSail)
            {
                Phase = GameplayPhase.Loading;
            }
        }

        public bool TryBoard(bool isNearBoardingPoint)
        {
            if (Phase != GameplayPhase.ReadyToSail || !isNearBoardingPoint)
            {
                return false;
            }

            Phase = GameplayPhase.Sailing;
            return true;
        }

        public bool TryDeliver(bool isInsideDeliveryZone)
        {
            if (Phase != GameplayPhase.Sailing || !isInsideDeliveryZone)
            {
                return false;
            }

            Phase = GameplayPhase.Delivered;
            return true;
        }

        public void Reset()
        {
            Phase = GameplayPhase.Loading;
        }
    }
}
