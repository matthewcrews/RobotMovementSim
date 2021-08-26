namespace RobotMovementSim

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open MonoGame.Extended.Tiled
open MonoGame.Extended.Tiled.Renderers
open MonoGame.Extended.ViewportAdapters

open State


type Game1 () as this =
    inherit Game()
 
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable robotSprite = Unchecked.defaultof<_>
    let mutable bots = Unchecked.defaultof<_>
    let mutable tiledMap = nil<_>
    let mutable tiledMapRenderer = nil<_>
    //let mutable camera = nil<_>
    //let mutable cameraPosition = nil<_>

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true

    //member _.GetMovementDirection () =
    //    let mutable movementDirection = Vector2.Zero
    //    let state = Keyboard.GetState ()
        
    //    if state.IsKeyDown Keys.Down then
    //        movementDirection <- movementDirection - Vector2.UnitY
        
    //    if state.IsKeyDown Keys.Up then
    //        movementDirection <- movementDirection + Vector2.UnitY

    //    if state.IsKeyDown Keys.Left then
    //        movementDirection <- movementDirection + Vector2.UnitX

    //    if state.IsKeyDown Keys.Right then
    //        movementDirection <- movementDirection - Vector2.UnitX

    //    if movementDirection <> Vector2.Zero then
    //        movementDirection.Normalize ()

    //    movementDirection


    //member this.MoveCamera (gameTime: GameTime) =
    //    let speed = 200f
    //    let seconds = gameTime.GetElapsedSeconds ()
    //    let movementDirection = this.GetMovementDirection()
    //    cameraPosition <- cameraPosition + speed * movementDirection * seconds


    override this.Initialize() =
        // TODO: Add your initialization logic here
        this.Window.AllowUserResizing <- true
        //let viewportAdapter = new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 800, 600)
        //camera <- OrthographicCamera (viewportAdapter)
        base.Initialize()



    override this.LoadContent() =
        // TODO: use this.Content to load your game content here
        robotSprite <- this.Content.Load<Texture2D>("RobotTriangle")
        let circleBot2 = {
            BotId = 2
            Position = Vector2(50f, 50f)
            Rotation = 0f
            State = Stopped
            Steps = 
                [
                    Step (Direction.Right, 100f)
                    Step (Direction.Down, 100f)
                    Step (Direction.Right, 100f)
                    Step (Direction.Up, 100f)
                    Step (Direction.Down, 100f)
                ]
        }
        bots <- [circleBot2]

        tiledMap <- this.Content.Load<TiledMap>("samplemap")
        tiledMapRenderer <- new TiledMapRenderer(this.GraphicsDevice, tiledMap)
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)


 
    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit();

        // TODO: Add your update logic here
        tiledMapRenderer.Update (gameTime)
        //this.MoveCamera gameTime
        //camera.LookAt cameraPosition
        bots <- bots |> List.map (Bot.move gameTime)

        base.Update(gameTime)
 


    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        // TODO: Add your drawing code here
        spriteBatch.Begin()
        
        tiledMapRenderer.Draw()
        //tiledMapRenderer.Draw(camera.GetInverseViewMatrix ())

        for bot in bots do
            //let sourceRect = Rectangle (0, 0, robotSprite.Width, robotSprite.Height)
            spriteBatch.Draw (robotSprite, bot.Position, nil<_>, Color.White, bot.Rotation, bot.Origin, Vector2.One, SpriteEffects.None, 1f)

        spriteBatch.End()
        base.Draw(gameTime)

