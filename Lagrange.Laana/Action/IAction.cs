using Laana;
using Lagrange.Core;

namespace Lagrange.Laana.Action
{
    public interface IAction<in TActionPing, TActionPong>
    {
        public Task<TActionPong> Handle(BotContext bot, TActionPing ping);
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ActionHandlerOf(LaanaActionPing.PingOneofCase action) : Attribute
    {
        public LaanaActionPing.PingOneofCase Action { get; } = action;
    }
}