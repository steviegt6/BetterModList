using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using TomatoLib;
using TomatoLib.Core.MonoModding;

namespace BetterModList.Content
{
    public abstract class PatchSystem : ModSystem
    {
        public static Assembly Terraria => typeof(Main).Assembly;

        public new TomatoMod Mod => (TomatoMod) base.Mod;

        public void Edit(MethodInfo method, string methodName) =>
            Mod.CreateEdit(method, GetType(), methodName);

        public void Detour(MethodInfo modifiedMethod, MethodInfo modifyingMethod) =>
            Mod.CreateDetour(modifiedMethod, modifyingMethod);
    }
}