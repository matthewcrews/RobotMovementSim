namespace RobotMovementSim

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open MonoGame.Extended.Tiled
open MonoGame.Extended.Tiled.Renderers
open MonoGame.Extended.ViewportAdapters
open State


[<AutoOpen>]
module MonoGameExtensions =
    type Viewport with
        member this.Center =
            Vector2(float32 this.Width * 0.5f, float32 this.Height * 0.5f)


type Camera(viewport: Viewport) =       
    member val WorldToScreen = Matrix.Identity with get, set
    member val ScreenToWorld = Matrix.Identity with get, set
    member val Zoom = 1.0f with get, set
    member val Position = Vector2.Zero with get, set
    member val Rotation = 0.0f with get, set

    member _.GetMovementDirection () =
        let mutable movementDirection = Vector2.Zero
        let state = Keyboard.GetState ()
        
        if state.IsKeyDown Keys.Down then
            movementDirection <- movementDirection + Vector2.UnitY
        
        if state.IsKeyDown Keys.Up then
            movementDirection <- movementDirection - Vector2.UnitY

        if state.IsKeyDown Keys.Left then
            movementDirection <- movementDirection - Vector2.UnitX

        if state.IsKeyDown Keys.Right then
            movementDirection <- movementDirection + Vector2.UnitX

        if movementDirection <> Vector2.Zero then
            movementDirection.Normalize ()

        movementDirection


    member this.MoveCamera (gameTime: GameTime) =
        let speed = 800f
        let seconds = gameTime.GetElapsedSeconds ()
        let movementDirection = this.GetMovementDirection()
        this.Position <- this.Position + speed * movementDirection * seconds


    member this.Update (gameTime: GameTime) =
        this.MoveCamera gameTime
        this.WorldToScreen <-
            Matrix.CreateTranslation(Vector3(-this.Position, 0.0f)) *
            Matrix.CreateRotationZ(this.Rotation ) *
            Matrix.CreateScale(Vector3(this.Zoom, this.Zoom, 1.f )) *
            Matrix.CreateTranslation(Vector3(viewport.Center, 0.f))
        this.ScreenToWorld <- Matrix.Invert(this.WorldToScreen)


type Game1 () as this =
    inherit Game()
 
    let graphics = new GraphicsDeviceManager(this, PreferredBackBufferWidth = 1920, PreferredBackBufferHeight = 1080)
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable robotSprite = Unchecked.defaultof<_>
    let mutable bots = Unchecked.defaultof<_>
    let mutable tiledMap = nil<_>
    let mutable tiledMapRenderer = nil<_>
    let mutable camera = nil<_>

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true


    override this.Initialize() =
        // TODO: Add your initialization logic here
        this.Window.AllowUserResizing <- true
        camera <- Camera(this.GraphicsDevice.Viewport)

        base.Initialize()



    override this.LoadContent() =
        // TODO: use this.Content to load your game content here
        robotSprite <- this.Content.Load<Texture2D>("RobotTriangle")
        let circleBot2 = {
            BotId = 2
            Position = Vector2(0f, 0f)
            Rotation = 0f
            State = Stopped
            Steps = 
                [
                    //Step (Direction.Right, 100f)
                    //Step (Direction.Down, 100f)
                    //Step (Direction.Right, 100f)
                    //Step (Direction.Up, 100f)
                    //Step (Direction.Down, 100f)
                ]
        }
        bots <- [circleBot2]

        tiledMap <- this.Content.Load<TiledMap>("CaveMap")
        tiledMapRenderer <- new TiledMapRenderer(this.GraphicsDevice, tiledMap)
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)


 
    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit();

        // TODO: Add your update logic here
        tiledMapRenderer.Update (gameTime)
        bots <- bots |> List.map (Bot.move gameTime)
        camera.Update gameTime
        base.Update(gameTime)
 


    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.Gray

        // TODO: Add your drawing code here
        spriteBatch.Begin(transformMatrix = Nullable.op_Implicit camera.WorldToScreen)
        tiledMapRenderer.Draw(camera.WorldToScreen)
        for bot in bots do
            spriteBatch.Draw (robotSprite, bot.Position, nil<_>, Color.White, bot.Rotation, bot.Origin, Vector2.One, SpriteEffects.None, 1f)

        spriteBatch.End()
        base.Draw(gameTime)

