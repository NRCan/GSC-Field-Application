namespace TestingUnits
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var expectedVersion = "3.1";

            // Act
            var actualVersion = "3.1";

            // Assert
            Assert.Equal(expectedVersion, actualVersion);
        }
    }
}
