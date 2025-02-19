// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using SixLabors.ImageSharp.Formats.Webp.Lossy;
using SixLabors.ImageSharp.Tests.TestUtilities;
using Xunit;

namespace SixLabors.ImageSharp.Tests.Formats.WebP
{
    [Trait("Format", "Webp")]
    public class Vp8ResidualTests
    {
        private static void RunSetCoeffsTest()
        {
            // arrange
            var residual = new Vp8Residual();
            short[] coeffs = { 110, 0, -2, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0 };

            // act
            residual.SetCoeffs(coeffs);

            // assert
            Assert.Equal(9, residual.Last);
        }

        [Fact]
        public void RunSetCoeffsTest_Works() => RunSetCoeffsTest();

#if SUPPORTS_RUNTIME_INTRINSICS
        [Fact]
        public void RunSetCoeffsTest_WithHardwareIntrinsics_Works() => FeatureTestRunner.RunWithHwIntrinsicsFeature(RunSetCoeffsTest, HwIntrinsics.AllowAll);

        [Fact]
        public void RunSetCoeffsTest_WithoutHardwareIntrinsics_Works() => FeatureTestRunner.RunWithHwIntrinsicsFeature(RunSetCoeffsTest, HwIntrinsics.DisableHWIntrinsic);
#endif
    }
}
