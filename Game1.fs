namespace RobotMovementSim

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open State


type Game1 () as this =
    inherit Game()
 
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable robotSprite = Unchecked.defaultof<_>
    let botCount = 1
    let mutable bots = Unchecked.defaultof<_>

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
        //let circleBot1 = {
        //    BotId = 1
        //    Position = Vector2(0f, 0f)
        //    Rotation = 0f
        //    State = Stopped
        //    Steps = [Right 100f; Down 100f; Left 100f; Up 100f]
        //}
        let circleBot2 = {
            BotId = 2
            Position = Vector2(50f, 50f)
            Rotation = MathHelper.PiOver4
            State = Stopped
            Steps = [Right 100f; Down 100f; Left 100f; Up 100f]
        }
        bots <- [circleBot2]
 
    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit();

        // TODO: Add your update logic here
        bots <- bots |> List.map (Bot.move gameTime)

        base.Update(gameTime)
 
    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue

        // TODO: Add your drawing code here
        spriteBatch.Begin()

        for bot in bots do
            //let sourceRect = Rectangle (0, 0, robotSprite.Width, robotSprite.Height)
            spriteBatch.Draw (robotSprite, bot.Position, nil<_>, Color.White, bot.Rotation, bot.Origin, Vector2.One, SpriteEffects.None, 1f)

        spriteBatch.End()
        base.Draw(gameTime)

