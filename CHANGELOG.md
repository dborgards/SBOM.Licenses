# [1.2.0](https://github.com/dborgards/SBOM.Licenses/compare/v1.1.0...v1.2.0) (2025-12-04)


### Features

* add GitHub API integration for license downloads ([85a8327](https://github.com/dborgards/SBOM.Licenses/commit/85a83276a23faa31c149f5e46e2a469ac8c30ed4))

# [1.1.0](https://github.com/dborgards/SBOM.Licenses/compare/v1.0.0...v1.1.0) (2025-12-04)


### Bug Fixes

* add CultureInvariant to regex pattern matching ([de147cf](https://github.com/dborgards/SBOM.Licenses/commit/de147cfcf012f342d4925fa8cf4094726240cba9))


### Features

* add configurable package exclusion patterns ([45fe48e](https://github.com/dborgards/SBOM.Licenses/commit/45fe48ed4d49283de449acd55c8cecfe199c6e03))


### Performance Improvements

* optimize package exclusion performance ([03027a2](https://github.com/dborgards/SBOM.Licenses/commit/03027a216ac399f3d46ca08032565e36a2121756))
* reduce memory allocation in package filtering ([3924b54](https://github.com/dborgards/SBOM.Licenses/commit/3924b54446cbc307c98bf1ed37fddd34f179ed92))

# 1.0.0 (2025-12-04)


### Bug Fixes

* add missing package-lock.json for npm ci ([daba8f6](https://github.com/dborgards/SBOM.Licenses/commit/daba8f6776eb4163eb8903f55e2860f3e3521d4a))
* pass PROJECT_PATH environment variable to semantic-release ([2ea0ef1](https://github.com/dborgards/SBOM.Licenses/commit/2ea0ef1ef37d3ece12372932780b45e3a8fde30c))
* use hardcoded project path in semantic-release config ([3f6bf2e](https://github.com/dborgards/SBOM.Licenses/commit/3f6bf2e341a323d6e07311bc295729c71f3bde61))


### Features

* implement semantic-release for automated releases ([5365d50](https://github.com/dborgards/SBOM.Licenses/commit/5365d505ebbd59ca5d19bae4f07234052a642cb3))


### BREAKING CHANGES

* Versioning is now handled by semantic-release based on conventional commits instead of MinVer
