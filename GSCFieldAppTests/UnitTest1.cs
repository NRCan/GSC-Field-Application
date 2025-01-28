using GSCFieldApp.ViewModel;

namespace GSCFieldAppTests
{
    public class UnitTest1
    {

        [Fact]
        public void Test1()
        {
            // Arrange
            var expectedVersion = "3.0.3";

            // Act
            var actualVersion = "3.0.3";

            // Assert
            Assert.Equal(expectedVersion, actualVersion);
        }
    }
}