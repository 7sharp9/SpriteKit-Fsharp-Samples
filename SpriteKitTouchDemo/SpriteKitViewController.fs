namespace SpriteKitSingleView

open System
open System.Drawing
open MonoTouch.Foundation
open MonoTouch.UIKit
open MonoTouch.SpriteKit
open MonoTouch.CoreGraphics

module spritekit =
    type SKEmitterNode with
        static member fromResource res =
            let emitterpath = NSBundle.MainBundle.PathForResource (res, "sks")
            NSKeyedUnarchiver.UnarchiveFile(emitterpath) :?> SKEmitterNode

open spritekit

[<Register ("SpriteKitViewController")>]
type SpriteKitViewController () as x =
    inherit UIViewController ()
    
    let mutable scene = Unchecked.defaultof<SKScene>
    let mutable spriteView = new SKView()
      
    let setupScene() =
        spriteView.Bounds <- RectangleF(0.f, 0.f, x.View.Bounds.Width * UIScreen.MainScreen.Scale, 
                                                  x.View.Bounds.Height * UIScreen.MainScreen.Scale)
        spriteView.ShowsDrawCount <- true
        spriteView.ShowsNodeCount <- true
        spriteView.ShowsFPS <- true
            
        x.View <- spriteView
        scene <- new SKScene (spriteView.Bounds.Size, 
                              BackgroundColor = UIColor.Black,
                              ScaleMode = SKSceneScaleMode.AspectFit)
                              
        //add stars
        use stars = SKEmitterNode.fromResource "Stars"
        stars.Position <- PointF(scene.Frame.GetMidX(), scene.Frame.GetMaxY())
        scene.AddChild(stars)
        
        //add spaceship
        use sprite = new SKSpriteNode ("Art/viper_mark_vii.png")
        sprite.Position <- PointF (scene.Frame.GetMidX (), scene.Frame.GetMidY ())
        sprite.Name <- "Ship"
        scene.AddChild(sprite)
        
        //add exaust
        use flame = SKEmitterNode.fromResource "Fire"
        flame.Position <- PointF(0.f, -60.f)
        sprite.AddChild(flame) 
        
    [<Export("PanSelector")>]
    let OnLabelPan( sender: UIGestureRecognizer) =
        match sender with
        | :? UIPanGestureRecognizer as pan ->
            match pan.State with
            | UIGestureRecognizerState.Changed ->
                let movement = pan.TranslationInView(x.View)
                let move = SKAction.MoveBy(movement.X * 1.75f, -movement.Y * 1.75f, 0.05)
                let ship = scene.GetChildNode("Ship")
                ship.RunAction(move)
                pan.SetTranslation(PointF.Empty, x.View)
            | _ -> ()
        | _ -> ()
      
    let setupGestures() =
        use panRecogniser = new UIPanGestureRecognizer(x, MonoTouch.ObjCRuntime.Selector("PanSelector"))
        x.View.AddGestureRecognizer(panRecogniser)

    override x.DidReceiveMemoryWarning () =
        base.DidReceiveMemoryWarning ()

    override x.ViewDidLoad () =
        base.ViewDidLoad()
        setupScene()
        setupGestures()

    override x.ShouldAutorotateToInterfaceOrientation (orientation) =
        orientation <> UIInterfaceOrientation.PortraitUpsideDown
        
    override x.ViewDidAppear(animated) =
        base.ViewDidDisappear (animated)
        spriteView.PresentScene(scene)
        
    override x.ViewDidDisappear(animated) =
        base.ViewDidDisappear (animated)
        scene.RemoveAllChildren()
        scene.RemoveAllActions() 