using ICities;
using ImprovedStopSelection.Detour;
using UndergroundStopsEnabler.RedirectionFramework;

namespace ImprovedStopSelection
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);
            Redirector<TransportToolDetour>.Deploy();
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            Redirector<TransportToolDetour>.Revert();
        }
    }
}