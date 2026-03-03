using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.VidPlayer;

public interface IVidPlayerStyleground {
    public VidPlayerCore? Core { get; }
    public bool Visible { get; }

    void RenderHires(Scene scene);
    
    internal static void ILLevelRender(ILContext il) {
        ILCursor cursor = new(il);

        // (after SetRenderTarget(null), Clear())
        // Render BG hi-res stylegrounds
        // (Right after End())
        // Render FG hi-res stylegrounds
        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdnull(),
                instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget")) ||
                !cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<GraphicsDevice>("Clear"))) {
            throw new InvalidOperationException("Cannot find SetRenderTarget(null) and/or Clear()!");
        }

        // (after SetRenderTarget(null), Clear())
        // Render BG hi-res stylegrounds
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(0); // emit false
        cursor.EmitDelegate(RenderHiresVPS);

        if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SpriteBatch>("End"))) {
            throw new InvalidOperationException("Cannot find SpriteBatch.End()!");
        }

        // (Right after End())
        // Render contents of said separate RenderTarget
        cursor.EmitLdarg0();
        cursor.EmitLdcI4(1); // emit true
        cursor.EmitDelegate(RenderHiresVPS);
    }

    private static void RenderHiresVPS(Level level, bool fg) {
        bool sbBegin = false;
        foreach (Backdrop backdrop in fg ? level.Foreground.Backdrops : level.Background.Backdrops) {
            if (backdrop is not IVidPlayerStyleground vps || !(vps.Core?.Hires ?? false)) continue;
            // XXX: This is a workaround that Maddie's Helping Hand also does. GameplayBuffers.Level is
            // cleared with BackgroundColor before drawing anything -- this means that background hires stylegrounds
            // will be completely blocked by the default black color.
            // A cleaner solution would probably be to replace the BackgroundColor in the original call to
            // Color.Transparent, and then draw the black background later when GameplayBuffers.Level is
            // rendered.
            //
            // The only drawback of this workaround (that I know of) is that, from the player/mapper's perspective,
            // the black background of the level will now be unaffected by colorgrading/etc.
            level.BackgroundColor = Color.Transparent;

            if (!sbBegin) {
                Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, GameplayHudRenderer.GetMatrix());
                sbBegin = true;
            }

            if (vps.Visible) {
                vps.RenderHires(level);
            }
        }
        if (sbBegin)
            Draw.SpriteBatch.End();
    }
}