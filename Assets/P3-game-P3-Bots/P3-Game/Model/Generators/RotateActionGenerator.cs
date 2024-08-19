using Model.Actions;
using System.Collections.Generic;

namespace Model.Generators
{
    public class RotateActionGenerator : IGenerator
    {

        private Game game;

        public RotateActionGenerator()
        {
            game = ActionGenerator.Instance.Game;
        }

        public List<IAction> GenerateActions(Card card)
        {
            List<IAction> actions = new();
            List<Placement> placements = game.GameGrid.Placements;
            foreach (Placement placement in placements)
            {
                if (true)
                {
                    Orientation curOrientation = placement.Orientation;
                    switch (curOrientation)
                    {
                        case Orientation.Up:
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Left));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Right));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Down));
                            break;                       
                                                         
                        case Orientation.Right:           
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Left));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Up));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Down));
                            break;                       

                        case Orientation.Down:           
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Left));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Right));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Up));
                            break;                    
                                                    
                        case Orientation.Left:     
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Up));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Right));
                            actions.Add(new RotateAction((Tricks.Rotate)card,game, placement, Orientation.Down));
                            break;
                    }
                }                
            }

            return actions;
        }
    }
}
