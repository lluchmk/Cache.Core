# [1.1.0](https://github.com/lluchmk/Cache.Core/compare/v1.0.3...v1.1.0) (2019-01-28)


### Bug Fixes

* **CI pipeline:** Rename Notify job to NotifyVersion and take variables from Pack job instead of build since the last one is not a dependency. ([6ad2161](https://github.com/lluchmk/Cache.Core/commit/6ad2161))
* **semantic release:** removed unneeded assignation of default values on .releaserc. ([0757089](https://github.com/lluchmk/Cache.Core/commit/0757089))


### Features

* Update pipeline definition and semantic release config to use semantic-release-ado instead of exec ([21d9fb5](https://github.com/lluchmk/Cache.Core/commit/21d9fb5))

## [1.0.3](https://github.com/lluchmk/Cache.Core/compare/v1.0.2...v1.0.3) (2018-12-02)


### Bug Fixes

* **CI pipeline:** Rename Notify job to NotifyVersion and take variables from Pack job instead of build since the last one is not a dependency. ([d7fe2b5](https://github.com/lluchmk/Cache.Core/commit/d7fe2b5))

## [1.0.2](https://github.com/lluchmk/Cache.Core/compare/v1.0.1...v1.0.2) (2018-12-02)


### Bug Fixes

* **CI pipeline:** Update to notify on both build and pack. ([c3cdd57](https://github.com/lluchmk/Cache.Core/commit/c3cdd57))

## [1.0.1](https://github.com/lluchmk/Cache.Core/compare/v1.0.0...v1.0.1) (2018-12-02)


### Bug Fixes

* **CI pipeline:** Update variables on Notify job to take values from Pakc instead of Build. ([b544a6c](https://github.com/lluchmk/Cache.Core/commit/b544a6c))





'##vso[task.setvariable variable=nextRelease]1.0.1

# 1.0.0 (2018-12-02)


### Features

* **semantic release:** Add semantic release taks ([974b412](https://github.com/lluchmk/Cache.Core/commit/974b412))





'##vso[task.setvariable variable=nextRelease]1.0.0
