using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EveAutoRat.Classes
{
  public class ActionThreadNewsRAT : ActionThread
  {
    private BlobCounter objectCounter = new BlobCounter();

    private ActionState currentState;
    private PixelStateEveEchoes eveEchoesState = null;
    private PixelStateWeapons weaponsState = null;
    private PixelStateInStation insideState = null;
    private PixelStateCargo cargoState = null;
    private PixelStateEnemies enemiesState = null;
    private ActionStateStartUp startUpAction = null;

    public bool isInStation = false;
    public int enemyCount = -1;
    public int enemyTargetedCount = -1;

    public ActionThreadNewsRAT(EveAutoRatMainForm parentForm, IntPtr emuHWnd, IntPtr eventHWnd) : base(parentForm, emuHWnd, eventHWnd)
    {
      eveEchoesState = new PixelStateEveEchoes(this);
      insideState = new PixelStateInStation(this);
      weaponsState = new PixelStateWeapons(this);
      cargoState = new PixelStateCargo(this);
      enemiesState = new PixelStateEnemies(this);

      startUpAction = new ActionStateStartUp(this, 100);

      currentState = new ActionStateNOP(this, 100);
      currentState
        .SetNextState(new ActionStateUnloadCargo(this, 2000))
        .SetNextState(new ActionStateStartEncounter(this, 500))
        .SetNextState(new ActionStateConfirmUntilEncounter(this, 100))
        .SetNextState(new ActionStateBattle(this, 100))
        .SetNextState(new ActionStateLoot(this, 1000))
        .SetNextState(currentState)
      ;

      List<PixelObject> poList = PixelObjectList.GetPixelObjectList(0);
    }

    public bool EveEchosIconFound
    {
      get
      {
        return eveEchoesState.IconFound;
      }
    }

    public InsideFlag InsideState
    {
      get
      {
        return insideState.CurrentState;
      }
    }

    public Dictionary<string, WeaponState> WeaponsState
    {
      get
      {
        return weaponsState.WeaponsState;
      }
    }

    public double CurrentHoldAmount
    {
      get
      {
        return cargoState.CurrentHoldAmount;
      }
    }

    

    protected override void StepEvery(Bitmap screenBmp, double totalTime)
    {
      eveEchoesState.StepEvery(screenBmp, totalTime);
      insideState.StepEvery(screenBmp, totalTime);
      weaponsState.StepEvery(screenBmp, totalTime);
      cargoState.StepEvery(screenBmp, totalTime);
      enemiesState.StepEvery(screenBmp, totalTime);
    }

    protected override double Step(double totalTime)
    {
      Bitmap currentThreshHoldBmp;
      int currentThreshHold = 64;
      if (currentState != null)
      {
        currentThreshHold = currentState.GetThreshHold();
      }
      currentThreshHoldBmp = threshHoldDictionary[currentThreshHold];

      if (EveEchosIconFound && currentState != startUpAction)
      {
        startUpAction.SetNextState(currentState);
        currentState = startUpAction;
      }
      else
      {

        if (currentState != null)
        {
          currentState = currentState.Run(totalTime);
          if (currentState != null)
          {
            return currentState.GetDelay();
          }
          currentState = currentState.Run(totalTime);
          if (currentState != null)
          {
            return currentState.GetDelay();
          }
        }
      }
      return 100.0;
    }

    public override void Draw(Bitmap b)
    {
      using (Graphics g = Graphics.FromImage(b))
      {
        weaponsState.Draw(g);
        if (currentState != null)
        {
          currentState.Draw(g);
        }
      }
    }
  }
}
