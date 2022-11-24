using System;
using TaleWorlds.Library;

namespace Adoption
{
    internal static class Debug
    {
        public static void Print(string message)
        {
            ModSettings settings = new();
            if (settings.Debug)
            {
                Color color = new(0.6f, 0.2f, 1f);
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
        }

        public static void Error(Exception exception)
        {
            InformationManager.DisplayMessage(new InformationMessage($"{SubModule.ModTitle}: {exception.Message}", Colors.Red));
        }
    }
}