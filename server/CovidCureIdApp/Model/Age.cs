using System;

namespace CovidCureIdApp.Model
{
    /// <summary>
    ///     Simple struct to process an age string value into an upper and lower bound.
    /// </summary>
    public struct Age
    {
        private int _ageLower;
        private int _ageUpper;

        /// <summary>
        ///     The lower bound age range represented by the string.
        /// </summary>
        public int Lower => _ageLower;

        /// <summary>
        ///     The upper bound age range represented by the string.
        /// </summary>
        public int Upper => _ageUpper;

        /// <summary>
        ///     Creates an instance of the age range based on the input string
        /// </summary>
        /// <param name="ageRange">A string value representing an age range.</param>
        public Age(string ageRange) {
            if(string.IsNullOrEmpty(ageRange)) {
                _ageLower = 0;
                _ageUpper = 100;

                return;
            }

            string[] ageParts = ageRange.Split('-');

            if(ageParts.Length == 2) {
                _ageLower = Convert.ToInt32(ageParts[0]);
                _ageUpper = Convert.ToInt32(ageParts[1].Split(' ')[0]);

                return;
            }

            // The age is either "<1 year" or "90+ years" or empty
            if(ageRange.Equals("<1 year", StringComparison.InvariantCultureIgnoreCase)) {
                _ageLower = 0;
                _ageUpper = 1;

                return;
            }

            _ageLower = 90;
            _ageUpper = 100;
        }
    }
}