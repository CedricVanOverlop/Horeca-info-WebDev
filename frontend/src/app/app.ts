import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SideMenuComponent } from './components/side-menu/side-menu.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SideMenuComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = 'horeca-info';
}
