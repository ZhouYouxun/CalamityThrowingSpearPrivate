using System;
using CalamityMod.Items.Weapons.DraedonsArsenal;
using CalamityMod.Items.Weapons.Melee;
using CalamityThrowingSpear.Weapons.ChangedWeapons.DPreDog.ElementalLanceC;
using Terraria;
using Terraria.ModLoader;

namespace CalamityThrowingSpear.Global
{
    public class CTSRecipeGroups : ModSystem
    {
        public static string ElementalLanceGroup => GetWeaponPairGroupName(typeof(ElementalLanceJav));
        public static string BotanicPiercerGroup => GetWeaponPairGroupName(typeof(Weapons.ChangedWeapons.CPreMoodLord.BotanicPiercerC.BotanicPiercerJav));
        public static string NadirGroup => GetWeaponPairGroupName(typeof(Weapons.ChangedWeapons.EAfterDog.NadirC.NadirJav));

        public override void AddRecipeGroups()
        {
            for (int i = 0; i < SwitchWeapons.WeaponSetA.Count && i < SwitchWeapons.WeaponSetB.Count; i++)
            {
                int firstItemId = GetItemType(SwitchWeapons.WeaponSetA[i]);
                int secondItemId = GetItemType(SwitchWeapons.WeaponSetB[i]);
                string groupName = GetWeaponPairGroupName(SwitchWeapons.WeaponSetA[i]);

                RecipeGroup group = new RecipeGroup(
                    () => $"{Lang.GetItemNameValue(firstItemId)} / {Lang.GetItemNameValue(secondItemId)}",
                    firstItemId,
                    secondItemId
                );

                group.IconicItemId = firstItemId;
                RecipeGroup.RegisterGroup(groupName, group);
            }
        }

        public override void PostAddRecipes()
        {
            ReplaceIngredientWithGroup(ModContent.ItemType<Nadir>(), ModContent.ItemType<VanishingPoint>(), ElementalLanceGroup);
            ReplaceIngredientWithGroup(ModContent.ItemType<VanishingPoint>(), ModContent.ItemType<BotanicPiercer>(), BotanicPiercerGroup);
        }

        public static string GetWeaponPairGroupName(Type weaponType)
        {
            string name = weaponType.Name;
            if (name.EndsWith("Jav", StringComparison.Ordinal))
            {
                name = name.Substring(0, name.Length - 3);
            }

            return $"CalamityThrowingSpear:RecipeGroup{name}";
        }

        private static int GetItemType(Type itemType)
        {
            return (int)typeof(ModContent)
                .GetMethod(nameof(ModContent.ItemType), Type.EmptyTypes)
                .MakeGenericMethod(itemType)
                .Invoke(null, null);
        }

        private static void ReplaceIngredientWithGroup(int resultItemId, int oldIngredientId, string groupName)
        {
            if (!RecipeGroup.recipeGroupIDs.TryGetValue(groupName, out int groupId))
            {
                return;
            }

            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                Recipe recipe = Main.recipe[i];
                if (!recipe.HasResult(resultItemId))
                {
                    continue;
                }

                int ingredientIndex = IngredientIndex(recipe, oldIngredientId);
                if (ingredientIndex == -1)
                {
                    continue;
                }

                int stack = recipe.requiredItem[ingredientIndex].stack;
                recipe.RemoveIngredient(oldIngredientId);
                recipe.AddRecipeGroup(groupId, stack);
            }
        }

        private static int IngredientIndex(Recipe recipe, int itemId)
        {
            for (int i = 0; i < recipe.requiredItem.Count; i++)
            {
                if (recipe.requiredItem[i].type == itemId)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
