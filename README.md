# Linq to ObservableCollection

Linq to ObservableCollection provides to WPF developers an additional tool when dealing with collections and observable items.

[![Build Status](https://zumten.visualstudio.com/_apis/public/build/definitions/d6fe51c2-2715-43c8-8bff-5cb5575470b4/3/badge)](https://zumten.visualstudio.com/ZumtenSoft/_build/index?definitionId=3)

## Introduction

This project aims to provide to WPF and Silverlight developers an additional tool when playing with collections and observable items. Linq to ObservableCollection provides different blocks that can be combined to create new behaviors. This allows richer interaction between your Models, ViewModels and Views.

## How it works?

You simply have to use the same Linq methods you are used to, but this time on ObservableCollections. The new collections built this way will be automatically observable.

The following example demonstrates how easy it is to create observator collections. Out of the original list, a new list of view models will be built by filtering only the ones with a name starting with an "A"


## How to install?

The library is currently only released through NuGet https://www.nuget.org/packages/ZumtenSoft.Linq2ObsCollection/


## Build your first LINQ to ObservableCollection

```csharp
IObservableCollection<ModelItem> models = new ObservableCollection<ModelItem>();
IObservableCollection<ViewModelItem> viewModels =
    from model in models
    where model.Name.StartsWith("A")
    select new ViewModelItem(model);

listView.ItemsSource = viewModels;

// Manipulate the list of models ...
```

_For more informations on what is supported and what is not, see the documentation._
_Note: An additional collection has been built for solving multithreading issues. See DispatcherQueue and DispatchObservatorCollection._