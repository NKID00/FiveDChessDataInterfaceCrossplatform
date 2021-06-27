﻿using FiveDChessDataInterface.Variants;
using System;

namespace DataInterfaceConsole.Actions
{
    class LoadPredefinedVariant : BaseAction
    {
        public override string Name => "Load Predefined Variant [ONLINE]";

        protected override void Run()
        {
            WaitForIngame();

            if (!GithubVariantGetter.IsCached)
                WriteLineIndented("Getting variants from github...");
            var variants = GithubVariantGetter.GetAllVariants();

            WriteLineIndented("Select a variant from the following:");
            for (int i = 0; i < variants.Length; i++)
            {
                WriteLineIndented($"{(i + 1).ToString().PadLeft((int)Math.Ceiling(Math.Log10(variants.Length)))}. {variants[i].Name} by {variants[i].Author}");
            }

            if (int.TryParse(Util.ConsoleReadLineWhile(() => this.di.IsValid()), out int input) && input > 0 && input <= variants.Length)
            {
                var chosenVariant = variants[input - 1];
                WriteLineIndented($"Loading variant '{chosenVariant.Name}'...");
                var gb = chosenVariant.GetGameBuilder();
                this.di.SetChessBoardArrayFromBuilder(gb);
                WriteLineIndented($"Variant loaded and written to memory.");
            }
            else
            {
                WriteLineIndented("Invalid input. Not loading any variant.");
            }
        }
    }
}