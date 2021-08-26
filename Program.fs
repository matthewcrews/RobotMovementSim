namespace RobotMovementSim

module Program =

    open System
    open Microsoft.Xna.Framework

    [<EntryPoint>]
    let main argv =
        use simulation = new Simulation()
        simulation.Run()
        0 // return an integer exit code
