namespace SystemsOfLinearEquations
{
    public class Config
    {
        public int formatVersion = 1;

        public IntroductionConfig introduction;
        public TestConfig test;
        public ExamplesConfig examples;

        public Template[] templates;

        public enum TaskTarget
        {
            None,
            PickAnswer,
            MatchingSolution,
            MatchingNumberOfSolutions,
            PickNumberOfSolutions,
        }

        public class Template : Scenario
        {
            public bool requireUserSystemOfEquations = false;
            public bool requireTargetSystemOfEquations = false;
        }

        public class Scenario
        {
            public string templateName = "default";

            public bool showNumberOfUnknownsAndEquations = false;
            public bool showCoefficientMatrices = false;
            public bool showCoefficientMatrixRank = false;
            public bool showAugmentedMatrices = false;
            public bool showAugmentedMatrixRank = false;

            public bool showContradictionsError = false;
            public bool showZeroedEquationsError = false;
            public bool showZeroedUnknownsError = false;
            public bool showSolutionOutsideVisualizationAreaInfo = false;

            public bool showKroneckerCapelliTheoremEquations = false;
            public bool showNumberOfSolutions = false;
            public bool showSolutionText = false;

            public string taskTextKey;
            public string taskText;
            public string[] taskTextParams;
            public TaskTarget taskTarget = TaskTarget.None;

            public bool showSystemOfEquations = false;

            public SystemOfEquationsDescription userSystemOfEquations;
            public SystemOfEquationsDescription targetSystemOfEquations;

            public bool showHoloSystemOfEquations = false;
            public bool showHoloSystemOfEquationsSolution = false;
            public bool showHoloTargetSystemOfEquationsSolution = false;

            public string[] answers;
            public int correctAnswer = 0;
            public bool shuffleAnswers = true;

            public int points = 1;
        }

        public class SystemOfEquationsDescription
        {
            public string systemCode = null;
        }


        public class IntroductionConfig
        {
            public string templateName = null;
            public Scenario[] scenarios;
        }

        public class TestConfig
        {
            public Scenario[] scenarios;
        }

        public class ExamplesConfig
        {
            public Scenario[] scenarios;
        }
    }
}
