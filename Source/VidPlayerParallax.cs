using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Celeste.Mod.VidPlayer;

//[CustomBackdrop("VidPlayer/VidPlayerParallax")]
public class VidPlayerParallax : Parallax, IVidPlayerStyleground {
    public VidPlayerCore? Core => inner.Core;
    bool IVidPlayerStyleground.Visible => base.Visible;
    private VidPlayerStyleground inner;
    private readonly MTexture fallback;
    private float scale;

    internal static Backdrop? OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element? above) {
        if (child.Name == "VidPlayer/VidPlayerParallax") {
            return new VidPlayerParallax(child, above);
        }
        return null;
    }

    public VidPlayerParallax(BinaryPacker.Element child, BinaryPacker.Element? above) : base(new MTexture(new VirtualTexture("Filler for Parallax", 1, 1, Color.White))) {
        UseSpritebatch = false; // TODO: Potential perf issue
        inner = new VidPlayerStyleground(child);
        fallback = GFX.Game["__fallback"];
        bool additive = child.HasAttr("blendmode") 
            ? child.Attr("blendmode") == "additive" : 
            (above != null && above.Attr("blendmode", "alphablend") == "additive");
        if (additive) {
            BlendState = BlendState.Additive;
        }
        DoFadeIn = bool.Parse(child.Attr("fadeIn", "false"));
        scale = child.AttrFloat("scale", 1f);
        // Everything else is handled by MapData.ParseBackdrop
    }
    
    public override void Update(Scene scene) {
        Position += Speed * Engine.DeltaTime;
        Position += WindMultiplier * (scene as Level)!.Wind * Engine.DeltaTime;
        if (DoFadeIn)
            fadeIn = Calc.Approach(fadeIn, Visible ? 1f : 0.0f, Engine.DeltaTime);
        else
            fadeIn = Visible ? 1f : 0.0f;
        inner.Update(scene);
        inner.Visible = Visible;
    }

    public override void Render(Scene scene) {
        if (Core == null) return;
        if (Core.Hires) return;
        RenderInner(scene, true);
    }
    
    public void RenderHires(Scene scene) {
        if (Core == null) return;
        if (!Core.Hires) return;
        RenderInner(scene, false);
    }
    
    private void RenderInner(Scene scene, bool doSb) {
        bool isDisposed = Core!.CheckDisposed();
        if (isDisposed || !Core.HasChromaKey) {
            Texture.Texture.Texture = isDisposed ? fallback.Texture.Texture : Core.videoPlayer!.GetTexture();
            if (doSb)
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null);
            new_Render(scene);
            if (doSb)
                Draw.SpriteBatch.End();
            return;
        }
        // ChromaKey path
        Texture.Texture.Texture = Core.videoPlayer!.GetTexture();
        Core.ChromaKeyPreRender(BlendState, !doSb);
        new_Render(scene);
        Core.ChromaKeyPostRender(!doSb);
    }

    public void new_Render(Scene scene) {
        float finalScale = scale;
        Vector2 screenSize = Core!.GetEntitySize();
        if (Core!.Hires) {
            //finalScale *= Core.CurrScaleFactor;
            screenSize *= Core.CurrScaleFactor;
        }
        float texWidth = Texture.Texture.Texture.Width * finalScale;
        float texHeight = Texture.Texture.Texture.Height * finalScale;
        Vector2 camPos = ((scene as Level)!.Camera.Position + CameraOffset).Floor();
        Vector2 startPos = ((Position - camPos * Scroll)*(Core.Hires ? Core.CurrScaleFactor : 1)).Floor();
        float num = fadeIn * Alpha * FadeAlphaMultiplier;
        if (FadeX != null)
            num *= FadeX.Value(camPos.X + screenSize.X/2f);
        if (FadeY != null)
            num *= FadeY.Value(camPos.Y + screenSize.Y/2f);
        Color color = Color;
        if (num < 1.0)
            color *= num;
        if (color.A <= 1)
            return;
        if (LoopX) {
            while (startPos.X < 0.0)
                startPos.X += texWidth;
            while (startPos.X > 0.0)
                startPos.X -= texWidth;
        }
        if (LoopY) {
            while (startPos.Y < 0.0)
                startPos.Y += texHeight;
            while (startPos.Y > 0.0)
                startPos.Y -= texHeight;
        }
        SpriteEffects flip = SpriteEffects.None;
        if (FlipX && FlipY)
            flip = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
        else if (FlipX)
            flip = SpriteEffects.FlipHorizontally;
        else if (FlipY)
            flip = SpriteEffects.FlipVertically;
        for (float x = startPos.X; x < screenSize.X; x += texWidth) {
            for (float y = startPos.Y; y < screenSize.Y; y += texHeight) {
                Draw.SpriteBatch.Draw(Texture.Texture.Texture, new Vector2(x, y), null, color * Core.GlobalAlpha, 0.0f, Vector2.Zero, finalScale, flip, 0.0f);
                if (!LoopY)
                    break;
            }
            if (!LoopX)
                break;
        }
    }

    public override void Ended(Scene scene) {
        base.Ended(scene);
        inner.Ended(scene);
    }

}