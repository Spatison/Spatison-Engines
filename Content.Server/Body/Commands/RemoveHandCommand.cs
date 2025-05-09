using System.Linq;
using Content.Server._White.Body.Systems;
using Content.Server.Administration;
using Content.Shared._White.Body;
using Content.Shared._White.Body.Components;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Random;

namespace Content.Server.Body.Commands
{
    [AdminCommand(AdminFlags.Fun)]
    public sealed class RemoveHandCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;

        public string Command => "removehand";
        public string Description => "Removes a hand from your entity.";
        public string Help => $"Usage: {Command}";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine("Only a player can run this command.");
                return;
            }

            if (player.AttachedEntity == null)
            {
                shell.WriteLine("You have no entity.");
                return;
            }

            if (!_entManager.TryGetComponent(player.AttachedEntity, out BodyComponent? body))
            {
                var text = $"You have no body{(_random.Prob(0.2f) ? " and you must scream." : ".")}";

                shell.WriteLine(text);
                return;
            }

            var bodySystem = _entManager.System<BodySystem>();
            var hand = bodySystem.GetBodyParts(player.AttachedEntity.Value, body, BodyPart.Hands).FirstOrDefault(); // WD EDIT

            if (hand == default)
            {
                shell.WriteLine("You have no hands.");
            }
            else
            {
                _entManager.System<SharedTransformSystem>().AttachToGridOrMap(hand.Uid);
            }
        }
    }
}
