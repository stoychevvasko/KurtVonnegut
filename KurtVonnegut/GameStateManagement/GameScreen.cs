#region File Description

//-----------------------------------------------------------------------------
// GameScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace GameStateManagement
{
    /// <summary>
    /// A screen is a single layer that has update and draw logic, and which
    /// can be combined with other layers to build up a complex menu system.
    /// For instance the main menu, the options menu, the "are you sure you
    /// want to quit" message box, and the main game itself are all implemented
    /// as screens.
    /// </summary>
    public abstract class GameScreen
    {
        /// <summary>
        /// Normally when one screen is brought up over the top of another,
        /// the first screen will transition off to make room for the new
        /// one. This property indicates whether the screen is only a small
        /// popup, in which case screens underneath it do not need to bother
        /// transitioning off.
        /// </summary>
        public bool IsPopup
        {
            get
            {
                return this.isPopup;
            }
            protected set
            {
                this.isPopup = value;
            }
        }
        
        private bool isPopup = false;
        
        /// <summary>
        /// Indicates how long the screen takes to
        /// transition on when it is activated.
        /// </summary>
        public TimeSpan TransitionOnTime
        {
            get
            {
                return this.transitionOnTime;
            }
            protected set
            {
                this.transitionOnTime = value;
            }
        }
        
        private TimeSpan transitionOnTime = TimeSpan.Zero;
        
        /// <summary>
        /// Indicates how long the screen takes to
        /// transition off when it is deactivated.
        /// </summary>
        public TimeSpan TransitionOffTime
        {
            get
            {
                return this.transitionOffTime;
            }
            protected set
            {
                this.transitionOffTime = value;
            }
        }
        
        private TimeSpan transitionOffTime = TimeSpan.Zero;
        
        /// <summary>
        /// Gets the current position of the screen transition, ranging
        /// from zero (fully active, no transition) to one (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionPosition
        {
            get
            {
                return this.transitionPosition;
            }
            protected set
            {
                this.transitionPosition = value;
            }
        }
        
        private float transitionPosition = 1;
        
        /// <summary>
        /// Gets the current alpha of the screen transition, ranging
        /// from 1 (fully active, no transition) to 0 (transitioned
        /// fully off to nothing).
        /// </summary>
        public float TransitionAlpha
        {
            get
            {
                return 1f - this.TransitionPosition;
            }
        }
        
        /// <summary>
        /// Gets the current screen transition state.
        /// </summary>
        public ScreenState ScreenState
        {
            get
            {
                return this.screenState;
            }
            protected set
            {
                this.screenState = value;
            }
        }
        
        private ScreenState screenState = ScreenState.TransitionOn;
        
        /// <summary>
        /// There are two possible reasons why a screen might be transitioning
        /// off. It could be temporarily going away to make room for another
        /// screen that is on top of it, or it could be going away for good.
        /// This property indicates whether the screen is exiting for real:
        /// if set, the screen will automatically remove itself as soon as the
        /// transition finishes.
        /// </summary>
        public bool IsExiting
        {
            get
            {
                return this.isExiting;
            }
            protected internal set
            {
                this.isExiting = value;
            }
        }
        
        private bool isExiting = false;
        
        /// <summary>
        /// Checks whether this screen is active and can respond to user input.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return !this.otherScreenHasFocus &&
                       (this.screenState == ScreenState.TransitionOn ||
                        this.screenState == ScreenState.Active);
            }
        }
        
        private bool otherScreenHasFocus;
        
        /// <summary>
        /// Gets the manager that this screen belongs to.
        /// </summary>
        public ScreenManager ScreenManager { get; internal set; }
        
        /// <summary>
        /// Gets the index of the player who is currently controlling this screen,
        /// or null if it is accepting input from any player. This is used to lock
        /// the game to a specific player profile. The main menu responds to input
        /// from any connected gamepad, but whichever player makes a selection from
        /// this menu is given control over all subsequent screens, so other gamepads
        /// are inactive until the controlling player returns to the main menu.
        /// </summary>
        public PlayerIndex? ControllingPlayer { get; internal set; }
        
        /// <summary>
        /// Gets the gestures the screen is interested in. Screens should be as specific
        /// as possible with gestures to increase the accuracy of the gesture engine.
        /// For example, most menus only need Tap or perhaps Tap and VerticalDrag to operate.
        /// These gestures are handled by the ScreenManager when screens change and
        /// all gestures are placed in the InputState passed to the HandleInput method.
        /// </summary>
        public GestureType EnabledGestures
        {
            get
            {
                return this.enabledGestures;
            }
            protected set
            {
                this.enabledGestures = value;
                
                // the screen manager handles this during screen changes, but
                // if this screen is active and the gesture types are changing,
                // we have to update the TouchPanel ourself.
                if (this.ScreenState == ScreenState.Active)
                {
                    TouchPanel.EnabledGestures = value;
                }
            }
        }
        
        private GestureType enabledGestures = GestureType.None;
        
        /// <summary>
        /// Gets whether or not this screen is serializable. If this is true,
        /// the screen will be recorded into the screen manager's state and
        /// its Serialize and Deserialize methods will be called as appropriate.
        /// If this is false, the screen will be ignored during serialization.
        /// By default, all screens are assumed to be serializable.
        /// </summary>
        public bool IsSerializable
        {
            get
            {
                return this.isSerializable;
            }
            protected set
            {
                this.isSerializable = value;
            }
        }
        
        private bool isSerializable = true;
        
        /// <summary>
        /// Activates the screen. Called when the screen is added to the screen manager or if the game resumes
        /// from being paused or tombstoned.
        /// </summary>
        /// <param name="instancePreserved">
        /// True if the game was preserved during deactivation, false if the screen is just being added or if the game was tombstoned.
        /// On Xbox and Windows this will always be false.
        /// </param>
        public virtual void Activate(bool instancePreserved)
        {
        }
        
        /// <summary>
        /// Deactivates the screen. Called when the game is being deactivated due to pausing or tombstoning.
        /// </summary>
        public virtual void Deactivate()
        {
        }
        
        /// <summary>
        /// Unload content for the screen. Called when the screen is removed from the screen manager.
        /// </summary>
        public virtual void Unload()
        {
        }
        
        /// <summary>
        /// Allows the screen to run logic, such as updating the transition position.
        /// Unlike HandleInput, this method is called regardless of whether the screen
        /// is active, hidden, or in the middle of a transition.
        /// </summary>
        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;
            
            if (this.isExiting)
            {
                // If the screen is going away to die, it should transition off.
                this.screenState = ScreenState.TransitionOff;
                
                if (!this.UpdateTransition(gameTime, transitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    this.ScreenManager.RemoveScreen(this);
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                if (this.UpdateTransition(gameTime, transitionOffTime, 1))
                {
                    // Still busy transitioning.
                    this.screenState = ScreenState.TransitionOff;
                }
                else
                {
                    // Transition finished!
                    this.screenState = ScreenState.Hidden;
                }
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                if (this.UpdateTransition(gameTime, transitionOnTime, -1))
                {
                    // Still busy transitioning.
                    this.screenState = ScreenState.TransitionOn;
                }
                else
                {
                    // Transition finished!
                    this.screenState = ScreenState.Active;
                }
            }
        }
        
        /// <summary>
        /// Helper for updating the screen transition position.
        /// </summary>
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta;
            
            if (time == TimeSpan.Zero)
            {
                transitionDelta = 1;
            }
            else
            {
                transitionDelta = (float)(gameTime.ElapsedGameTime.TotalMilliseconds / time.TotalMilliseconds);
            }

            // Update the transition position.
            this.transitionPosition += transitionDelta * direction;
            
            // Did we reach the end of the transition?
            if (((direction < 0) && (this.transitionPosition <= 0)) ||
                ((direction > 0) && (this.transitionPosition >= 1)))
            {
                this.transitionPosition = MathHelper.Clamp(this.transitionPosition, 0, 1);
                return false;
            }
            
            // Otherwise we are still busy transitioning.
            return true;
        }
        
        /// <summary>
        /// Allows the screen to handle user input. Unlike Update, this method
        /// is only called when the screen is active, and not when some other
        /// screen has taken the focus.
        /// </summary>
        public virtual void HandleInput(GameTime gameTime, InputState input)
        {
        }
        
        /// <summary>
        /// This is called when the screen should draw itself.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
        }
        
        /// <summary>
        /// Tells the screen to go away. Unlike ScreenManager.RemoveScreen, which
        /// instantly kills the screen, this method respects the transition timings
        /// and will give the screen a chance to gradually transition off.
        /// </summary>
        public void ExitScreen()
        {
            if (this.TransitionOffTime == TimeSpan.Zero)
            {
                // If the screen has a zero transition time, remove it immediately.
                this.ScreenManager.RemoveScreen(this);
            }
            else
            {
                // Otherwise flag that it should transition off and then exit.
                this.isExiting = true;
            }
        }
    }
}