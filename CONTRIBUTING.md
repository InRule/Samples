# Contributing

Please read our [Code of Conduct][CODE_OF_CONDUCT.md]

## Setup

### Fork on GitHub


Before you do anything else, login/signup on GitHub and fork [Samples][project]

### Clone your fork locally

If you have git-scm installed, you now clone your git repo using the following command-line argument where \<my-github-name> is your account name on GitHub::

    git clone git@github.com:<my-github-name>/Samples.git
___

## Issues

The list of outstanding InRule Samples feature requests and bugs can be found on our on our [GitHub Issue Tracker][issues]. Pick an unassigned issue that you think you can accomplish and add a comment that you are attempting to do it.

Feel free to propose issues that aren't described!

## Setting up topic branches and generating pull requests
___

While it's handy to provide useful code snippets in an issue, it is better for
you as a developer to submit pull requests. By submitting pull request your
contribution to InRule Samples will be recorded by Github.

In git it is best to isolate each topic or feature into a "topic branch".  While
individual commits allow you control over how small individual changes are made
to the code, branches are a great way to group a set of commits all related to
one feature together, or to isolate different efforts when you might be working
on multiple topics at the same time.

While it takes some experience to get the right feel about how to break up
commits, a topic branch should be limited in scope to a single ``issue`` as
submitted to an issue tracker.

Also since GitHub pegs and syncs a pull request to a specific branch, it is the
**ONLY** way that you can submit more than one fix at a time.  If you submit
a pull from your develop branch, you can't make any more commits to your develop
without those getting added to the pull.

To create a topic branch, its easiest to use the convenient ``-b`` argument to ``git
checkout``::

    git checkout -b fix-broken-thing
    Switched to a new branch 'fix-broken-thing'

You should use a verbose enough name for your branch so it is clear what it is
about.  Now you can commit your changes and regularly merge in the upstream
develop as described below.

When you are ready to generate a pull request, either for preliminary review,
or for consideration of merging into the project you must first push your local
topic branch back up to GitHub::

    git push origin fix-broken-thing

Now when you go to your fork on GitHub, you will see this branch listed under
the "Source" tab where it says "Switch Branches".  Go ahead and select your
topic branch from this list, and then click the "Pull request" button.

Here you can add a comment about your branch.  If this in response to
a submitted issue, it is good to put a link to that issue in this initial
comment.  The repo managers will be notified of your pull request and it will
be reviewed (see below for best practices).  Note that you can continue to add
commits to your topic branch (and push them up to GitHub) either if you see
something that needs changing, or in response to a reviewer's comments.  If
a reviewer asks for changes, you do not need to close the pull and reissue it
after making changes. Just make the changes locally, push them to GitHub, then
add a comment to the discussion section of the pull request.

## Pull upstream changes into your fork regularly
___

Others might be working in the same area as you, to avoid unfortunate collisions, please pull in upstream work before submitting any pull requests

To pull in upstream changes::

    git remote add upstream https://github.com/InRule/Samples.git
    git fetch upstream develop

Check the log to be sure that you actually want the changes, before merging::

    git log upstream/develop

Then merge the changes that you fetched::

    git merge upstream/develop

For more info, see http://help.github.com/fork-a-repo/

## How to get your pull request accepted

___

We want your submission. But we also want to provide a stable experience for our users and the community. Follow these rules and you should succeed without a problem!

### Add Tests

* If you add new Samples, add tests
* If you can't add tests, add detailed testing instructions

### Don't mix code changes with whitespace cleanup

If you change two lines of code and correct 200 lines of whitespace issues in a file the diff on that pull request is functionally unreadable and will be **rejected**. Whitespace cleanups need to be in their own pull request.

Keep your pull requests limited to a single issue
--------------------------------------------------

InRule Sample pull requests should be as small/atomic as possible. Large, wide-sweeping changes in a pull request will be **rejected**, with comments to isolate the specific code in your pull request.

## How pull requests are checked, tested, and done

___

First we pull the code into a local branch::

    git checkout -b <branch-name> <submitter-github-name
    git pull git://github.com/InRule/Samples.git develop

Then we run the tests


We finish with a merge and push to GitHub::

    git checkout develop
    git merge <branch-name>
    git push origin develop


[project]: https://github.com/InRule/Samples
[issues]: https://github.com/InRule/Samples/issues