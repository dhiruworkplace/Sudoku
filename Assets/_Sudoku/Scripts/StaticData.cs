using UnityEngine;

namespace ArtboxGames
{
    public class StaticData : MonoBehaviour
    {
        // ads id
#if UNITY_ANDROID || UNITY_EDITOR
        public static string appID = "ca-app-pub-6163322720080156~7424761582";
        public static string bannerUnitId = "ca-app-pub-6163322720080156/5920108221";
        public static string interstitialAdUnitId = "ca-app-pub-6163322720080156/9667781549";
        public static string rewardAdUnitId = "ca-app-pub-6163322720080156/2380511984";
        public static string rewardedInterstitialUnitId = "ca-app-pub-6163322720080156/2621529377";
        public static string appOpenUnitId = "ca-app-pub-6163322720080156/7348485014"; 
#endif
    }
}