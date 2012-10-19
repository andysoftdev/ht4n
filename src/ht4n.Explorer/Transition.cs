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

    /// <summary>
    /// The transition.
    /// </summary>
    internal class Transition : FrameworkElement
    {
        #region Static Fields

        /// <summary>
        /// The first property.
        /// </summary>
        public static DependencyProperty FirstProperty;

        /// <summary>
        /// The second property.
        /// </summary>
        public static DependencyProperty SecondProperty;

        /// <summary>
        /// The source property.
        /// </summary>
        public static DependencyProperty SourceProperty;

        /// <summary>
        /// The state property.
        /// </summary>
        public static DependencyProperty StateProperty;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Transition"/> class.
        /// </summary>
        static Transition()
        {
            SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(Transition), new PropertyMetadata((obj, args) => ((Transition)obj).Swap()));
            FirstProperty = DependencyProperty.Register("First", typeof(object), typeof(Transition));
            SecondProperty = DependencyProperty.Register("Second", typeof(object), typeof(Transition));
            StateProperty = DependencyProperty.Register("State", typeof(TransitionState), typeof(Transition), new PropertyMetadata(TransitionState.First));
        }

        #endregion

        #region Enums

        /// <summary>
        /// The transition state.
        /// </summary>
        public enum TransitionState
        {
            /// <summary>
            /// The first.
            /// </summary>
            First, 

            /// <summary>
            /// The second.
            /// </summary>
            Second
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the first.
        /// </summary>
        public object First
        {
            get
            {
                return this.GetValue(FirstProperty);
            }

            set
            {
                this.SetValue(FirstProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the second.
        /// </summary>
        public object Second
        {
            get
            {
                return this.GetValue(SecondProperty);
            }

            set
            {
                this.SetValue(SecondProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        public object Source
        {
            get
            {
                return this.GetValue(SourceProperty);
            }

            set
            {
                this.SetValue(SourceProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public TransitionState State
        {
            get
            {
                return (TransitionState)this.GetValue(StateProperty);
            }

            set
            {
                this.SetValue(StateProperty, value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The swap.
        /// </summary>
        private void Swap()
        {
            if (this.State == TransitionState.First)
            {
                this.Second = this.Source;
                this.State = TransitionState.Second;
            }
            else
            {
                this.First = this.Source;
                this.State = TransitionState.First;
            }
        }

        #endregion
    }
}