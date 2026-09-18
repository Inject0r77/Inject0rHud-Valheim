using Inject0rHUD.Models;
using UnityEngine;

namespace Inject0rHUD.Services
{
    internal static class ShipContextService
    {
        internal static ShipContextEntry Read(Player player)
        {
            if (player == null)
                return ShipContextEntry.Empty;

            try
            {
                // Match Valheim's native ship HUD: context is shown while the local
                // player is actively controlling a ship.
                Ship ship = player.GetControlledShip();
                if (ship == null)
                    return ShipContextEntry.Empty;

                float health = -1f;
                WearNTear wear = ship.GetComponent<WearNTear>();
                if (wear == null)
                    wear = ship.GetComponentInChildren<WearNTear>();
                if (wear != null)
                    health = Mathf.Clamp01(wear.GetHealthPercentage());

                float windIntensity = EnvMan.instance != null
                    ? Mathf.Clamp01(EnvMan.instance.GetWindIntensity())
                    : 0f;

                Ship.Speed speedSetting = ship.GetSpeedSetting();

                return new ShipContextEntry(
                    true,
                    GetShipName(ship),
                    Mathf.Abs(ship.GetSpeed()),
                    health,
                    speedSetting.ToString(),
                    NormalizeSignedAngle(ship.GetWindAngle()),
                    Mathf.Clamp01(ship.GetWindAngleFactor()),
                    windIntensity,
                    ship.GetRudderValue());
            }
            catch
            {
                return ShipContextEntry.Empty;
            }
        }

        private static string GetShipName(Ship ship)
        {
            if (ship == null)
                return string.Empty;

            try
            {
                Piece piece = ship.GetComponent<Piece>();
                if (piece != null && !string.IsNullOrEmpty(piece.m_name))
                {
                    if (global::Localization.instance != null)
                        return global::Localization.instance.Localize(piece.m_name);
                    return piece.m_name;
                }
            }
            catch
            {
            }

            return ship.gameObject != null
                ? ship.gameObject.name.Replace("(Clone)", string.Empty).Trim()
                : "Ship";
        }

        private static float NormalizeSignedAngle(float degrees)
        {
            degrees %= 360f;
            if (degrees < 0f) degrees += 360f;
            return degrees > 180f ? degrees - 360f : degrees;
        }
    }
}
