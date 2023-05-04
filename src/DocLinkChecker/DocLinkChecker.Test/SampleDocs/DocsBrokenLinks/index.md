# Broken documentation hierarchy

This hierarchy of documents is broken and should test with these errors with the DocLinkChecker:

## general\README.md

* Reference outside of the docs hierarchy: ../../README.md [line 8]

## general\general-sample.md

* Non existing URL: <https://nonexisting.loremipsum.io/generator/?n=5&t=p> [line 5]
* Non existing resource: ./images/nature-non-existing.jpeg [line 7]
* Non existing URL: <https://xyzdoesnotexist.com/blah> [line 9]
* Non existing URL: <https://www.non-existing-hanselman.com/blog/RemoteDebuggingWithVSCodeOnWindowsToARaspberryPiUsingNETCoreOnARM.aspx> [line 11]
* Non existing heading in another MD file: ./another-sample.md#fifth-header [line 13]

## general\images

* Unused resource: unused-image.jpeg

## getting-started\README.md

* Non existing folder: ../non-existing-folder/ [line 3]
* Non existing MD file: ../general/general-sample-non-existing.md [line 5]
