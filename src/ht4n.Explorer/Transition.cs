/** -*- C# -*-
 * Copyright (C) 2010-2012 Thalmann Software & Consulting, http://www.softdev.ch
 *
 * This file is part of ht4n.
 *
 * ht4n is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 3
 * of the License, or any later version.
 *
 * Hypertable is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

namespace Hypertable.Explorer
{
    using System.Windows;

    internal class Transition : FrameworkElement
    {
        #region Constants and Fields

        public static DependencyProperty FirstProperty;

        public static DependencyProperty SecondProperty;

        public static DependencyProperty SourceProperty;

        public static DependencyProperty StateProperty;

        #endregion

        #region Constructors and Destructors

        static Transition() {
            SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(Transition), new PropertyMetadata((obj, args) => ((Transition)obj).Swap()));
            FirstProperty = DependencyProperty.Register("First", typeof(object), typeof(Transition));
            SecondProperty = DependencyProperty.Register("Second", typeof(object), typeof(Transition));
            StateProperty = DependencyProperty.Register("State", typeof(TransitionState), typeof(Transition), new PropertyMetadata(TransitionState.First));
        }

        #endregion

        #region Enums

        public enum TransitionState
        {
            First, 

            Second
        }

        #endregion

        #region Public Properties

        public object First {
            get {
                return this.GetValue(FirstProperty);
            }

            set {
                this.SetValue(FirstProperty, value);
            }
        }

        public object Second {
            get {
                return this.GetValue(SecondProperty);
            }

            set {
                this.SetValue(SecondProperty, value);
            }
        }

        public object Source {
            get {
                return this.GetValue(SourceProperty);
            }

            set {
                this.SetValue(SourceProperty, value);
            }
        }

        public TransitionState State {
            get {
                return (TransitionState)this.GetValue(StateProperty);
            }

            set {
                this.SetValue(StateProperty, value);
            }
        }

        #endregion

        #region Methods

        private void Swap() {
            if (this.State == TransitionState.First) {
                this.Second = this.Source;
                this.State = TransitionState.Second;
            }
            else {
                this.First = this.Source;
                this.State = TransitionState.First;
            }
        }

        #endregion
    }
}