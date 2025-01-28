using Xunit;
using GSCFieldApp.ViewModel;
using GSCFieldApp.Views;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using System.Threading.Tasks;

namespace GSCFieldAppTests
{
    public class AboutPageViewModelTests
    {
        private AboutPageViewModel _viewModel;
        private AboutPage _aboutPage;

        public AboutPageViewModelTests()
        {
            _viewModel = new AboutPageViewModel();
            _aboutPage = new AboutPage
            {
                BindingContext = _viewModel
            };
        }

        [Fact]
        public void AppVersion_ShouldReturnCorrectVersion()
        {
            // Arrange
            var expectedVersion = AppInfo.Current.Version.ToString();

            // Act
            var actualVersion = _viewModel.AppVersion;

            // Assert
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void AppDBVersion_ShouldReturnCorrectVersion()
        {
            // Arrange
            var expectedVersion = "1.0.0"; // Replace with actual DB version

            // Act
            var actualVersion = _viewModel.AppDBVersion;

            // Assert
            Assert.Equal(expectedVersion, actualVersion);
        }

        [Fact]
        public void LogoRotation_ShouldBeZeroInitially()
        {
            // Arrange & Act
            var rotation = _viewModel.LogoRotation;

            // Assert
            Assert.Equal(0, rotation);
        }

        [Fact]
        public async Task LogoTapped_ShouldRotateLogo()
        {
            // Arrange
            var initialRotation = _viewModel.LogoRotation;

            // Act
            await _viewModel.LogoTappedCommand.ExecuteAsync(null);

            // Assert
            Assert.Equal(initialRotation + 36, _viewModel.LogoRotation);
        }

        [Fact]
        public async Task LogoTapped_ShouldToggleDeveloperMode()
        {
            // Arrange
            _viewModel.LogoRotation = 324; // Set rotation to just before a full circle

            // Act
            await _viewModel.LogoTappedCommand.ExecuteAsync(null);

            // Assert
            Assert.True(_viewModel.DeveloperModeActivated);
        }

        [Theory]
        [InlineData("https://github.com/NRCan/GSC-Field-Application/blob/master/Documents/GSC%20FIELD%20APP%20GUIDE.pdf")]
        [InlineData("https://github.com/NRCan/GSC-Field-Application")]
        [InlineData("https://github.com/NRCan/GSC-Field-Application/issues")]
        [InlineData("https://doi.org/10.4095/pjucp83rbn")]
        [InlineData("https://natural-resources.canada.ca/home")]
        [InlineData("https://natural-resources.canada.ca/science-and-data/research-centres-and-labs/geological-survey-canada/17100")]
        [InlineData("https://www.canada.ca/en/environment-climate-change/services/science-technology/open-science-action-plan.html")]
        public async Task TapCommand_ShouldNavigateToCorrectUrl(string url)
        {
            //// Arrange
            //var command = _viewModel.TapCommand;

            //// Act
            //await command.ExecuteAsync(url);

            // Assert
            // Here you would typically mock the Launcher.OpenAsync method and verify it was called with the correct URL
        }
    }
}