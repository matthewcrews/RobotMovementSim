namespace RobotMovementSim

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type Game1 () as this =
    inherit Game()
 
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable robotSprite = Unchecked.defaultof<_>
    let botCount = 1
    let mutable bots : State.Bots = Unchecked.defaultof<_>

    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true

    override this.Initialize() =
        // TODO: Add your initialization logic here
        
        base.Initialize()

    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        // TODO: use this.Content to load your game content here
        robotSprite <- this.Content.Load<Texture2D>("RobotTriangle")
        // Create a single bot and move it to the right
        bots <- State.Bots.create 1
        bots.AccX.[0] <- 0.0001f
        bots.State.[0] <- State.BotState.RightTo 200.0f
 
    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit();

        // TODO: Add your update logic here
        State.Bots.move bots gameTime.ElapsedGameTime

        base.Update(gameTime)
 
    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        // TODO: Add your drawing code here
        spriteBatch.Begin()

        for i = 0 to bots.LocX.Length - 1 do
            let position = Vector2 (bots.LocX.[i], bots.LocY.[i])
            spriteBatch.Draw (robotSprite, position, Color.White)

        spriteBatch.End()
        base.Draw(gameTime)

