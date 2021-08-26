namespace RobotMovementSim

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open MonoGame.Extended
open MonoGame.Extended.Tiled
open MonoGame.Extended.Tiled.Renderers
open MonoGame.Extended.ViewportAdapters
open RobotMovementSim
open RobotMovementSim.State


type Simulation () as this =
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
            Position = Vector2(2f * 32f, 3f *32f)
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

        tiledMapRenderer.Draw(camera.WorldToScreen)

        // TODO: Add your drawing code here
        spriteBatch.Begin(transformMatrix = Nullable.op_Implicit camera.WorldToScreen)
        
        for bot in bots do
            let color =
                match bot.State with
                | Idle -> Color.Yellow
                | _ -> Color.White

            spriteBatch.Draw (robotSprite, bot.Position, nil<_>, color, bot.Rotation, bot.Origin, Vector2.One, SpriteEffects.None, 1f)

        spriteBatch.End()
        base.Draw(gameTime)

