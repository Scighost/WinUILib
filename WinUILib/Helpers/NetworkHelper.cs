using Windows.Networking.Connectivity;

namespace Scighost.WinUILib.Helpers;

public static class NetworkHelper
{


    public static bool IsInternetOnMeteredConnection()
    {
        var profile = NetworkInformation.GetInternetConnectionProfile();
        if (profile is null)
        {
            return true;
        }
        var cost = profile.GetConnectionCost();
        return cost.NetworkCostType != NetworkCostType.Unrestricted;
    }


}
